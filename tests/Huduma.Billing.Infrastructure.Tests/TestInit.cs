using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Huduma.Billing.Infrastructure.Tests
{
    [SetUpFixture]
    public class TestInit
    {
        public static IServiceProvider ServiceProvider;

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
            
            services.AddInfrastructure(config);
            services.AddEventBus(config);
            
            ServiceProvider = services.BuildServiceProvider();
            
            InitDb();
        }

        private void InitDb()
        {
            var ct = ServiceProvider.GetService<BillingDbContext>();
            ct.Database.EnsureDeleted();
            ct.Database.EnsureCreated();
        }
    }
}