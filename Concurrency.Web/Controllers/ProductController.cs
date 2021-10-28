using Concurrency.Entities;
using Concurrency.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrency.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IServiceScopeFactory serviceScopeFactory;

        public ProductController(IServiceScopeFactory serviceScopeFactory)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        public IActionResult Index()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                    IEnumerable<Product> products = await productService.GetProducts();
                }
                catch(Exception ex)
                {
                    
                }

            });
            return View();
        }
    }
}
