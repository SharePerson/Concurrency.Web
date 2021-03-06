using AutoMapper;
using Concurrency.Entities;
using Concurrency.Services.Generic;
using Concurrency.Services.Interfaces;
using Concurrency.Services.Interfaces.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Concurrency.Services.Factories
{
    public class BookingGatewayFactory<InstanceType>: IBookingGatewayFactory<InstanceType> where InstanceType: IAsyncDisposable
    {
        public InstanceType Create()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();

            IServiceCollection services = new ServiceCollection()
                .AddDbContext<ConcurrencyDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), builder =>
                {
                    builder.MigrationsAssembly("Concurrency.Migrations");
                    builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
                }), ServiceLifetime.Transient)
                .AddTransient<ITaskBookingGateway, TaskBookingGateway>()
                .AddTransient<IBookingGateway, BookingGateway>()
                .AddTransient(typeof(IBookingGateway<,>), typeof(BookingGateway<,>))
                .AddTransient(typeof(IBookingGatewayFactory<>), typeof(BookingGatewayFactory<>));
                

            IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
            services.AddSingleton(mapper);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetRequiredService<InstanceType>();
        }

        public InstanceType CreateSqlite()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false);
            var configuration = builder.Build();

            IServiceCollection services = new ServiceCollection()
                .AddDbContext<ConcurrencyDbContext>(options => options.UseSqlite(configuration.GetConnectionString("SqliteConnection"), builder =>
                {
                    builder.MigrationsAssembly("Concurrency.Migrations.Sqlite");
                }), ServiceLifetime.Transient)
                .AddTransient<ITaskBookingGateway, TaskBookingGateway>()
                .AddTransient<IBookingGateway, BookingGateway>()
                .AddTransient(typeof(IBookingGateway<,>), typeof(BookingGateway<,>))
                .AddTransient(typeof(IBookingGatewayFactory<>), typeof(BookingGatewayFactory<>));

            IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
            services.AddSingleton(mapper);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetRequiredService<InstanceType>();
        }

        public InstanceType CreateScoped()
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
    }
}
