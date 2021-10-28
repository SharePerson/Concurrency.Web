using Concurrency.Entities;
using Concurrency.Entities.Banking;
using Concurrency.Repositories.Base;
using Concurrency.Repositories.Interfaces;

namespace Concurrency.Repositories
{
    public class TransactionRepository: RepositoryBase<Transaction>, ITransactionRepository
    {
        public TransactionRepository(ConcurrencyDbContext dbContext): base(dbContext)
        {

        }
    }
}
