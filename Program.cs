using System;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Hosting;
using Serilog.Sinks.Grafana.Loki;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

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
                    loggerConfiguration.WriteTo.Console();
                    var lokiUrl = Environment.GetEnvironmentVariable("LOKI_URL");
                    if (lokiUrl != null)
                    {
                        Console.WriteLine("Found LOKI_URL {0}", lokiUrl); 
                        loggerConfiguration.WriteTo.GrafanaLoki(
                            lokiUrl, 
                            labels: new [] { 
                                new LokiLabel {Key = "application", Value = "emulator-backend-8080"},
                            }
                        );
                    }
                })
                .ConfigureServices(services => 
                {
                    services.AddDbContext<CpuDbContext>(options =>
                        options.UseNpgsql(Environment.GetEnvironmentVariable("Database__ConnectionString"), b => b.MigrationsAssembly("emulator_backend_8080"))
                            .UseSnakeCaseNamingConvention()
                    );

                    services.AddHostedService<FetchExecuteService>();

                    services.AddHttpClient();
                });
        }
    }
}
