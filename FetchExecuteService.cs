using System;
using System.IO;
using System.Net.Http;
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

        public FetchExecuteService(ILogger<FetchExecuteService> logger, IHttpClientFactory httpClientFactory, CpuDbContext dbContext)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting fetch execute service for 8080 backend");

            while (!cancellationToken.IsCancellationRequested)
            {
                var unassignedComputer = await _dbContext.Computer.FirstOrDefaultAsync(c => c.Status == ComputerStatus.NotAssigned, cancellationToken);

                if (unassignedComputer != null)
                {
                    _logger.LogInformation("Found unassigned computer with id {0}", unassignedComputer.Id);
                    unassignedComputer.Status = ComputerStatus.Assigned;
                    await _dbContext.SaveChangesAsync();

                    await RunComputerAsync(unassignedComputer);
                }
                
                await Task.Delay(5000);
            }
        }

        private async Task RunComputerAsync(Computer computer)
        {
            if (computer == null) throw new ArgumentNullException(nameof(computer));

            // Create new CPU object
            var cpu = new Cpu();
            var cpuIsHalted = false;
            // TODO - Create microservice which can set initial values on CPU object

            var client = _httpClientFactory.CreateClient();

            // Start fetch execute loop
            var countdownToStatusUpdate = 1000;
            while (true)
            {
                if (cpuIsHalted) continue; // TODO - Don't spin wait on HLT waiting for interrupt

                cpu.Opcode = (await _dbContext.AddressSpace.SingleAsync(m => m.ComputerId == computer.Id && m.Address == cpu.State.ProgramCounter)).Value;

                var bytes = CpuStaticData.NumberOfBytesPerOpcode[cpu.Opcode];
                var opcode = CpuStaticData.OpcodeName[cpu.Opcode];
                _logger.LogInformation("{Id}: {Opcode} {CpuState}", computer.Id, opcode, cpu.State.ToString());
                cpu.State.ProgramCounter = (ushort)(cpu.State.ProgramCounter + bytes);

                switch (opcode)
                {
                    case Opcode.HLT:
                        cpuIsHalted = true;
                        break;
                    default:
                        // All other opcodes correspond directly to microservices
                        var url = CpuStaticData.OpcodeUrl[opcode] + "?";

                        for (var ii = 1; ii < bytes; ii++)
                        {
                            var operand = (await _dbContext.AddressSpace.SingleAsync(m => m.ComputerId == computer.Id && m.Address == cpu.State.ProgramCounter + ii)).Value;
                            url += $"operand{ii}={operand}&";
                        }
                        var response = await client.PostAsJsonAsync(url, cpu);
                        response.EnsureSuccessStatusCode();
                        cpu = await response.Content.ReadFromJsonAsync<Cpu>();
                        break;
                }

                countdownToStatusUpdate--;
                if (countdownToStatusUpdate == 0)
                {
                    countdownToStatusUpdate = 1000;
                    using (var stream = new MemoryStream())
                    {
                        await JsonSerializer.SerializeAsync(stream, cpu);
                        stream.Position = 0;
                        computer.State = await JsonDocument.ParseAsync(stream);
                        await _dbContext.SaveChangesAsync();
                    }
                }    
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
  }
}