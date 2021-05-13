using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace emulator_backend_8080
{
    internal class Program
    {
        public async static Task<int> Main(string[] args)
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .MinimumLevel.Warning()
                .WriteTo.Console();

            Log.Logger = logConfig.CreateLogger();

            try
            {
                Log.Information("Starting web host");
                await CreateHostBuilder(args).Build().RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
