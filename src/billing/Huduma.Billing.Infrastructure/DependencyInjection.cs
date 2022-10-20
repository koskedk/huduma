using System;
using System.Reflection;
using Huduma.Billing.Application;
using Huduma.Billing.Domain;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Huduma.Billing.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<HudumaOptions>(configuration.GetSection(HudumaOptions.BusKey));
            services.AddDb(configuration);
            services.AddScoped<IBillRepository, BillRepository>();
            return services;
        }
    
        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            var busSettings = configuration.GetSection(HudumaOptions.BusKey).Get<HudumaOptions>();
            var provider = configuration.GetSection("TransportProvider").Value;
        
            services.AddMassTransit(cfg =>
            {
                if (provider.ToUpper() == "InMemory".ToUpper())
                {
                    cfg.AddSagaStateMachine<BillStateMachine, BillState>()
                        .InMemoryRepository();
                
                    cfg.SetKebabCaseEndpointNameFormatter();
                    
                    cfg.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                }
            
                if (provider.ToUpper() == "RabbitMq".ToUpper())
                {
                
                    cfg.AddSagaStateMachine<BillStateMachine, BillState>()
                        .DapperRepository(GetDbConnection(configuration));
                    
                    cfg.SetKebabCaseEndpointNameFormatter();
                
                    cfg.UsingRabbitMq((context,cfg) =>
                    {
                        cfg.Host(busSettings.BusHost, busSettings.BusVhost, h => {
                            h.Username(busSettings.BusUser);
                            h.Password(busSettings.BusPass);
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                }
            });
        
            return services;
        }
    
        private static IServiceCollection AddDb(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = configuration.GetSection("DatabaseProvider").Value;

            if (provider.ToUpper() == "sqlite".ToUpper())
            {
                var cn = new SqliteConnection(configuration.GetConnectionString("sqliteConnection"));
                cn.Open();
                services.AddDbContext<BillingDbContext>(cfg => cfg.UseSqlite(cn));
            }

            if (provider.ToUpper() == "sqlserver".ToUpper())
            {
                var cns = configuration.GetConnectionString("sqlserverConnection");
                services.AddDbContext<BillingDbContext>(cfg =>
                {
                    cfg.UseSqlServer(cns,
                        x => x
                            .MigrationsAssembly(typeof(BillingDbContext).GetTypeInfo().Assembly.GetName().Name));
                });
            }
            return services;
        }

        private static string GetDbConnection(IConfiguration configuration)
        {
            var provider = configuration.GetSection("DatabaseProvider").Value;

            if (provider.ToUpper() == "sqlite".ToUpper())
            {
                return configuration.GetConnectionString("sqliteConnection");
            }

            if (provider.ToUpper() == "sqlserver".ToUpper())
            {
                return configuration.GetConnectionString("sqlserverConnection");
            }
            return configuration.GetConnectionString("sqliteConnection");
        }
    }
}