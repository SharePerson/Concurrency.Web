using Concurrency.Entities;
using Concurrency.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Concurrency.Services.Factories
{
    public static class BookingGatewayFactory
    {
        public static InstanceType Create<InstanceType>() where InstanceType: class
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddDbContext<ConcurrencyDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), builder =>
                {
                    builder.MigrationsAssembly("Concurrency.Migrations");
                    builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
                }), ServiceLifetime.Transient)
                .AddTransient<ITaskBookingGateway, TaskBookingGateway>()
                .AddTransient<IBookingGateway, BookingGateway>()
                .BuildServiceProvider();

            return serviceProvider.GetRequiredService<InstanceType>();
        }

        public static InstanceType CreateSqlite<InstanceType>() where InstanceType : class
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddDbContext<ConcurrencyDbContext>(options => options.UseSqlite(configuration.GetConnectionString("SqliteConnection"), builder =>
                {
                    builder.MigrationsAssembly("Concurrency.Migrations.Sqlite");
                }), ServiceLifetime.Transient)
                .AddTransient<ITaskBookingGateway, TaskBookingGateway>()
                .AddTransient<IBookingGateway, BookingGateway>()
                .BuildServiceProvider();

            return serviceProvider.GetRequiredService<InstanceType>();
        }

        public static InstanceType CreateScoped<InstanceType>() where InstanceType : class
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddDbContext<ConcurrencyDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), builder =>
                {
                    builder.MigrationsAssembly("Concurrency.Migrations");
                    builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
                }))
                .AddScoped<ITaskBookingGateway, TaskBookingGateway>()
                .BuildServiceProvider();

            return serviceProvider.GetRequiredService<InstanceType>();
        }

        public static async Task DisposeAsync<InstanceType>(InstanceType instance) where InstanceType: IAsyncDisposable
        {
            await instance.DisposeAsync();
        }
    }
}
