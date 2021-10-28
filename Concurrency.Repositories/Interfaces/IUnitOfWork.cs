using Concurrency.Entities;
using System;
using System.Threading.Tasks;

namespace Concurrency.Repositories.Interfaces
{
    public interface IUnitOfWork: IAsyncDisposable
    {
        ConcurrencyDbContext DbContext { get; }
        Task Commit();
    }
}
