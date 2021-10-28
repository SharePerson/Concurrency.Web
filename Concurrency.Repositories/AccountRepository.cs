using Concurrency.Entities;
using Concurrency.Entities.Banking;
using Concurrency.Repositories.Base;
using Concurrency.Repositories.Interfaces;

namespace Concurrency.Repositories
{
    public class AccountRepository: RepositoryBase<Account>, IAccountRepository
    {
        public AccountRepository(ConcurrencyDbContext dbContext): base(dbContext)
        {

        }
    }
}
