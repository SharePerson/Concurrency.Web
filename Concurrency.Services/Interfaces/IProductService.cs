using Concurrency.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Concurrency.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProducts();
    }
}
