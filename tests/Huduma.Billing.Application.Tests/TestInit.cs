using System;
using Huduma.Billing.Infrastructure;
using Huduma.Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Huduma.Billing.Application.Tests
{
    [SetUpFixture]
    public class TestInit
    {
        public static IServiceProvider ServiceProvider;
        public static ITestHarness TestHarness;

        [OneTimeSetUp]
        public void Init()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .Build();
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Quartz", LogEventLevel.Information)
                .WriteTo.Console()
                .CreateLogger();

            var services = new ServiceCollection();

            
            services.AddTestEventBus(config);
            services.AddApplication(config);
            services.AddInfrastructure(config);
            

            ServiceProvider = services.BuildServiceProvider();

            InitBus();
            InitDb();
        }

        private void InitBus()
        {
            TestHarness = ServiceProvider.GetTestHarness();
            TestHarness.Start().Wait();
        }

        private  void InitDb()
        {
            var ctx = ServiceProvider.GetService<BillingDbContext>();
            ctx.Database.EnsureDeleted();
            ctx.Database.EnsureCreated();
        }
    }

    public static class DependencyInjection
    {
        public static IServiceCollection AddTestEventBus(this IServiceCollection services,IConfiguration configuration)
        {
            
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.SetKebabCaseEndpointNameFormatter();
                    
                cfg.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
                
                cfg.AddRequestClient<CheckBill>();
                
                cfg.AddSagaStateMachine<BillStateMachine, BillState>();
            });
            return services;
        }
    }
    
}