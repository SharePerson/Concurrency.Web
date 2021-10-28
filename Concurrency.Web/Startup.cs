using Concurrency.Entities;
using Concurrency.Repositories;
using Concurrency.Repositories.Interfaces;
using Concurrency.Services;
using Concurrency.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Concurrency.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            //services.AddDbContext<ConcurrencyDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"), builder =>
            //{
            //    builder.MigrationsAssembly("Concurrency.Migrations");
            //}));

            services.AddDbContext<ConcurrencyDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("SqliteConnection"), builder =>
            {
                builder.MigrationsAssembly("Concurrency.Migrations.Sqlite");
            }));

            services.AddScoped<IProductService, ProductService>(sp =>
            {
                ConcurrencyDbContext dbContext = sp.GetRequiredService<ConcurrencyDbContext>();
                return new ProductService(dbContext);
            });
            services.AddScoped<IBookingService, BookingService>();

            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<ITransactionRepository, TransactionRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
