using Concurrency.Entities;
using Concurrency.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Concurrency.Services
{
    public class ProductService: IProductService
    {
        private readonly ConcurrencyDbContext _dbContext;

        public ProductService(ConcurrencyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _dbContext.Products.ToListAsync();
        }
    }
}
