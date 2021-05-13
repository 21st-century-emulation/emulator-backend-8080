using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using emulator_backend_8080.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace emulator_backend_8080.Services
{
    public class FetchExecuteService : BackgroundService
    {
        public ILogger<FetchExecuteService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly string _interruptServiceUrl;
        private readonly string _initialiseCpuServiceUrl;
        private readonly string _readRangeServiceUrl;
        private readonly FetchExecuteServiceQueue _fetchExecuteServiceQueue;

        public FetchExecuteService(ILogger<FetchExecuteService> logger, IHttpClientFactory httpClientFactory, FetchExecuteServiceQueue fetchExecuteServiceQueue)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
            };
            _interruptServiceUrl = Environment.GetEnvironmentVariable("INTERRUPT_SERVICE_URL"); // Interrupt service can be null in which case we're not checking for interrupts (Saving api call on each fetch execute cycle)
            _initialiseCpuServiceUrl = Environment.GetEnvironmentVariable("INITIALISE_CPU_SERVICE") ?? throw new ApplicationException("Application requires environment variable INITIALISE_CPU_SERVICE to be set");
            _readRangeServiceUrl = Environment.GetEnvironmentVariable("READ_RANGE_API") ?? throw new ApplicationException("Application requires environment variable READ_RANGE_API to be set");
            _fetchExecuteServiceQueue = fetchExecuteServiceQueue ?? throw new ArgumentNullException(nameof(fetchExecuteServiceQueue));
        }

        private async Task RunComputerAsync(string id, CancellationToken cancellationToken)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            var client = _httpClientFactory.CreateClient();

            var cpu = new Cpu { Id = id };
            var cpuIsHalted = false;

            // Initialise cpu object for computer it's running within
            cpu = await CallMicroserviceAsync(client, _initialiseCpuServiceUrl, cpu, cancellationToken);

            // Start fetch execute loop
            var countdownToStatusUpdate = 1000;
            int cyclesTaken = 0;

            while (true)
            {
                if (!string.IsNullOrWhiteSpace(_interruptServiceUrl))
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
                }

                if (cpuIsHalted) continue; // TODO - Don't spin wait on HLT waiting for interrupt

                (var opcodeByte, var operand1, var operand2) = await GetInstructionAsync(client, cpu, cancellationToken);

                cpu.Opcode = opcodeByte;
                var operands = new byte[] { operand1, operand2 };

                var bytes = CpuStaticData.NumberOfBytesPerOpcode[cpu.Opcode];
                var opcode = CpuStaticData.OpcodeName[cpu.Opcode];
                _logger.LogInformation("{Id}: {Opcode} {CpuState}", id, opcode.ToDebugString(cpu.Opcode, operands[0], operands[1]).PadRight(20), cpu.State.ToString());
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
                    _logger.LogInformation("Saving cpu state to database for analysis after 1000 cycles");
                    countdownToStatusUpdate = 1000;
                    await _fetchExecuteServiceQueue.SetStateAsync(cpu, cancellationToken);
                }
            }
        }

        private async Task<(byte, byte, byte)> GetInstructionAsync(HttpClient client, Cpu cpu, CancellationToken cancellationToken)
        {
            var response = await client.GetAsync($"{_readRangeServiceUrl}?id={cpu.Id}&address={cpu.State.ProgramCounter}&length=3");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var bytes = Convert.FromBase64String(responseString);

            return (bytes[0], bytes[1], bytes[2]);
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting fetch execute service for 8080 backend");

            while (!stoppingToken.IsCancellationRequested)
            {
                var id = await _fetchExecuteServiceQueue.GetComputerWaitingAsync(stoppingToken);

                if (id != null)
                {
                    _logger.LogInformation("Found unassigned computer with id {0}", id);

                    await RunComputerAsync(id, stoppingToken);
                }

                await Task.Delay(1000);
            }
        }
    }
}
