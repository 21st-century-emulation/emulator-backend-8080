using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EmulationDatabaseLibrary.Models;
using emulator_backend_8080.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace emulator_backend_8080
{
    public class FetchExecuteService : IHostedService
    {
        public ILogger<FetchExecuteService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CpuDbContext _dbContext;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly string _interruptServiceUrl;
        private readonly string _initialiseCpuServiceUrl;

        public FetchExecuteService(ILogger<FetchExecuteService> logger, IHttpClientFactory httpClientFactory, CpuDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
            };
            _interruptServiceUrl = Environment.GetEnvironmentVariable("INTERRUPT_SERVICE_URL") ?? throw new ApplicationException("Application requires environment variable INTERRUPT_SERVICE_URL to be set");
            _initialiseCpuServiceUrl = Environment.GetEnvironmentVariable("INITIALISE_CPU_SERVICE") ?? throw new ApplicationException("Application requires environment variable INITIALISE_CPU_SERVICE to be set");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _dbContext.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Starting fetch execute service for 8080 backend");

            while (!cancellationToken.IsCancellationRequested)
            {
                var unassignedComputer = await _dbContext.Computer.FirstOrDefaultAsync(c => c.Status == ComputerStatus.NotAssigned, cancellationToken);

                if (unassignedComputer != null)
                {
                    _logger.LogInformation("Found unassigned computer with id {0}", unassignedComputer.Id);
                    unassignedComputer.Status = ComputerStatus.Assigned;
                    await _dbContext.SaveChangesAsync();

                    await RunComputerAsync(unassignedComputer, cancellationToken);
                }
                
                await Task.Delay(5000);
            }
        }

        private async Task RunComputerAsync(Computer computer, CancellationToken cancellationToken)
        {
            if (computer == null) throw new ArgumentNullException(nameof(computer));

            var client = _httpClientFactory.CreateClient();

            // Create new CPU object
            var cpu = new Cpu
            {
                Id = computer.Id.ToString()
            };
            var cpuIsHalted = false;

            // Initialise cpu object for computer it's running within
            cpu = await CallMicroserviceAsync(client, _initialiseCpuServiceUrl, cpu, cancellationToken);

            // Start fetch execute loop
            var countdownToStatusUpdate = 1000;
            int cyclesTaken = 0;

            while (true)
            {
                var checkInterruptResponse = await client.PostAsync(
                    _interruptServiceUrl, 
                    new StringContent(JsonSerializer.Serialize(cpu, _jsonSerializerOptions))
                    {
                        Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                    });
                checkInterruptResponse.EnsureSuccessStatusCode();
                var checkInterruptResponseData = await checkInterruptResponse.Content.ReadAsStringAsync();
                if (byte.TryParse(checkInterruptResponseData, out var rstType))
                {
                    cpuIsHalted = false;
                    cpu.Opcode = rstType switch 
                    {
                        0 => 0xC7,
                        1 => 0xCF,
                        2 => 0xD7,
                        3 => 0xDF,
                        4 => 0xE7,
                        5 => 0xEF,
                        6 => 0xF7,
                        7 => 0xFF,
                        _ => throw new ApplicationException($"Invalid RST type {rstType}"),
                    };
                    _logger.LogInformation("Firing interrupt {RstType}", rstType);
                    cpu = await CallMicroserviceAsync(client, CpuStaticData.OpcodeUrl[Opcode.RST], cpu, cancellationToken);
                }

                if (cpuIsHalted) continue; // TODO - Don't spin wait on HLT waiting for interrupt

                var opcodeData = await _dbContext.AddressSpace.Where(
                    m => m.ComputerId == computer.Id && 
                    (m.Address == cpu.State.ProgramCounter || 
                     m.Address == ((cpu.State.ProgramCounter + 1) & 0xFFFF) || // Operand 1
                     m.Address == ((cpu.State.ProgramCounter + 2) & 0xFFFF))).ToListAsync();
                cpu.Opcode = opcodeData.Single(a => a.Address == cpu.State.ProgramCounter).Value;
                var operands = new [] 
                {
                    opcodeData.Single(a => a.Address == ((cpu.State.ProgramCounter + 1) & 0xFFFF)).Value,
                    opcodeData.Single(a => a.Address == ((cpu.State.ProgramCounter + 2) & 0xFFFF)).Value,
                };

                var bytes = CpuStaticData.NumberOfBytesPerOpcode[cpu.Opcode];
                var opcode = CpuStaticData.OpcodeName[cpu.Opcode];
                _logger.LogInformation("{Id}: {Opcode} {CpuState}", computer.Id, opcode.ToDebugString(cpu.Opcode, operands[0], operands[1]).PadRight(20), cpu.State.ToString());
                cpu.State.ProgramCounter = (ushort)(cpu.State.ProgramCounter + bytes);

                if (opcode == Opcode.HLT)
                {
                    cpuIsHalted = true;
                    continue;
                }

                var url = CpuStaticData.OpcodeUrl[opcode];

                if (bytes > 0)
                {
                    url += "?";
                    for (var ii = 1; ii < bytes; ii++)
                    {
                        url += $"operand{ii}={operands[ii - 1]}";

                        if (ii != bytes - 1) url += "&";
                    }
                }
                var previousCycles = cpu.State.Cycles;
                cpu = await CallMicroserviceAsync(client, url, cpu, cancellationToken);
                _logger.LogDebug("{0}", JsonSerializer.Serialize(cpu, _jsonSerializerOptions));
                cyclesTaken = (int)(cpu.State.Cycles - previousCycles);

                countdownToStatusUpdate -= cyclesTaken;
                if (countdownToStatusUpdate < 0)
                {
                    _logger.LogInformation("Saving cpu state to database for analysis");
                    countdownToStatusUpdate = 1000;
                    computer.State = JsonSerializer.Serialize(cpu, _jsonSerializerOptions);
                    await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);
                }    
            }
        }

        private async Task<Cpu> CallMicroserviceAsync(HttpClient client, string url, Cpu cpu, CancellationToken cancellationToken)
        {
            var response = await client.PostAsync(
                url, 
                new StringContent(JsonSerializer.Serialize(cpu, _jsonSerializerOptions))
                {
                    Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
                });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed request to {0} with response body {1}", url, await response.Content.ReadAsStringAsync());
                response.EnsureSuccessStatusCode();
            }
            
            return await response.Content.ReadFromJsonAsync<Cpu>(_jsonSerializerOptions, cancellationToken: cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
  }
}