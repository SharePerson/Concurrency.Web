using Concurrency.Entities;
using Concurrency.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace Concurrency.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConcurrencyDbContext dbContext;

        public ConcurrencyDbContext DbContext { get; private set; }

        public UnitOfWork(ConcurrencyDbContext dbContext)
        {
            this.dbContext = dbContext;
            DbContext = dbContext;
        }

        public async Task Commit()
        {
            await dbContext.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await dbContext.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
