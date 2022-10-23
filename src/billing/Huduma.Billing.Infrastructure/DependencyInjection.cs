﻿using System;
using System.Reflection;
using Huduma.Billing.Application;
using Huduma.Billing.Domain;
using Huduma.Contracts;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Huduma.Billing.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<HudumaTransportOptions>(configuration.GetSection(HudumaTransportOptions.Key));
            services.AddDb(configuration);
            services.AddStateDb(configuration);
            services.AddScoped<IBillRepository, BillRepository>();
            return services;
        }
    
        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            var busSettings = configuration.GetSection(HudumaTransportOptions.Key).Get<HudumaTransportOptions>();
            var provider = configuration.GetSection("TransportProvider").Value;
        
            services.AddMassTransit(cfg =>
            {
                if (provider.ToUpper() == "InMemory".ToUpper())
                {
                    cfg.AddSagaStateMachine<BillStateMachine, BillState>()
                        .InMemoryRepository();
                    cfg.AddRequestClient<CheckBill>();
                
                    cfg.SetKebabCaseEndpointNameFormatter();
                    
                    cfg.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                }
            
                if (provider.ToUpper() == "RabbitMq".ToUpper())
                {
                
                    cfg.AddSagaStateMachine<BillStateMachine, BillState>()
                        .EntityFrameworkRepository(r =>
                        {
                            r.ConcurrencyMode = ConcurrencyMode.Pessimistic; // or use Optimistic, which requires RowVersion
                            r.ExistingDbContext<BillingStateDbContext>();
                        });
                    
                    cfg.AddRequestClient<CheckBill>();
                    
                    cfg.SetKebabCaseEndpointNameFormatter();
                
                    cfg.UsingRabbitMq((context,cfg) =>
                    {
                        cfg.Host(busSettings.Host, busSettings.Vhost, h => {
                            h.Username(busSettings.User);
                            h.Password(busSettings.Pass);
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
        
        private static IServiceCollection AddStateDb(this IServiceCollection services, IConfiguration configuration)
        {
            var provider = configuration.GetSection("DatabaseProvider").Value;

            if (provider.ToUpper() == "sqlite".ToUpper())
            {
                var cn = new SqliteConnection(configuration.GetConnectionString("sqliteStateConnection"));
                cn.Open();
                services.AddDbContext<BillingDbContext>(cfg => cfg.UseSqlite(cn));
            }

            if (provider.ToUpper() == "sqlserver".ToUpper())
            {
                var cns = configuration.GetConnectionString("sqlserverStateConnection");
                services.AddDbContext<DbContext, BillingStateDbContext>((provider, builder) =>
                {
                    builder.UseSqlServer(cns, m =>
                    {
                        m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                        m.MigrationsHistoryTable($"__{nameof(BillingStateDbContext)}");
                    });
                });
            }
            return services;
        }


        private static string GetDbConnection(IConfiguration configuration)
        {
            var provider = configuration.GetSection("DatabaseProvider").Value;

            if (provider.ToUpper() == "sqlite".ToUpper())
                return configuration.GetConnectionString("sqliteConnection");

            if (provider.ToUpper() == "sqlserver".ToUpper())
                return configuration.GetConnectionString("sqlserverConnection");

            return configuration.GetConnectionString("sqliteConnection");
        }
        
        public static void SetupDb(this IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var provider = configuration.GetSection("DatabaseProvider").Value;
            using var scope=serviceProvider.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            if (provider.ToUpper() == "sqlite".ToUpper())
                ctx.Database.EnsureCreated();
            else
                ctx.Database.Migrate();
        }
        
        public static void SetupStateDb(this IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var provider = configuration.GetSection("DatabaseProvider").Value;
            using var scope=serviceProvider.CreateScope();
            using var ctx = scope.ServiceProvider.GetRequiredService<BillingStateDbContext>();
            if (provider.ToUpper() == "sqlite".ToUpper())
                ctx.Database.EnsureCreated();
            else
                ctx.Database.Migrate();
        }
    }
}
