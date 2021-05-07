using System;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Hosting;
using Serilog.Sinks.Grafana.Loki;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog.Events;

namespace emulator_backend_8080
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder();
            await host.RunConsoleAsync();
            return Environment.ExitCode;
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .UseSerilog((hostContext, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                        .WriteTo.Console();
                    var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL");
                    if (lokiUrl != null)
                    {
                        Console.WriteLine("Found LOKI_URL {0}", lokiUrl); 
                        loggerConfiguration
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                            .WriteTo.GrafanaLoki(
                                lokiUrl, 
                                outputTemplate: "[{Timestamp:HH:mm:ss.ffffff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                                labels: new [] { 
                                    new LokiLabel {Key = "application", Value = "emulator-backend-8080"},
                                }
                            );
                    }
                })
                .ConfigureServices(services => 
                {
                    services.AddDbContext<CpuDbContext>(options =>
                        options.UseNpgsql(Environment.GetEnvironmentVariable("Database__ConnectionString"), b => b.MigrationsAssembly("EmulationDatabaseLibrary"))
                            .UseSnakeCaseNamingConvention()
                    );

                    services.AddHostedService<FetchExecuteService>();

                    services.AddHttpClient();
                });
        }
    }
}
