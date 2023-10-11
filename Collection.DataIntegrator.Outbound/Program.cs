using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Collection.DataIntegrator.Outbound.Models;
using Collection.DataIntegrator.Outbound.Business;
using Collection.DataIntegrator.Outbound.DAL.UnitOfWork;
using Collection.DataIntegrator.Outbound.DAL.Interface;
using Collection.DataIntegrator.Outbound.Settings;
using Collection.DataIntegrator.Outbound;
using Collection.DataIntegrator.Outbound.Common.Interface;
using Collection.DataIntegrator.Outbound.Common;

namespace collection.dataintegrator.outbound
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var app = CreateHostBuilder(args).Build();

            var logger = app.Services.GetService<ILogger<Program>>();
            try
            {
                logger.LogInformation("Starting up the service at {time}", DateTimeOffset.Now);
                app.Run();
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failure Starting up the service");
                return;
            }
            finally
            {
                //logger.CloseAndFlush();
            }
           
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // configure the app here.
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<CommunicationQContext>(options =>
                    options.UseSqlServer(hostContext.Configuration.GetConnectionString("CommunicationQ")));
                    services.AddTransient<IUnitOfWork<CommunicationQContext>,UnitOfWork<CommunicationQContext>>();
                    services.AddTransient(typeof(OutboundDataQueueBL));
                    services.AddTransient(typeof(EventTypeBL));
                    services.Configure<Uploading>(options => hostContext.Configuration.GetSection(Uploading.SectionName).Bind(options)); 
                    services.AddHostedService<Uploader>();
                })
                .ConfigureLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddLog4Net();
                });
    }
}
