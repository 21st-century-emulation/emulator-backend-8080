using System.Text.Json;
using emulator_backend_8080.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace emulator_backend_8080
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IHostEnvironment env)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            services.AddControllers().AddJsonOptions((options) => 
            {
                options.JsonSerializerOptions.IncludeFields = true;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
            services.AddSingleton<FetchExecuteServiceQueue>();
            services.AddHostedService<FetchExecuteService>();
            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseHealthChecks("/status");
            app.UseSerilogRequestLogging();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/status");
                endpoints.MapControllers();
            });
        }
    }
}
