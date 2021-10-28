using Concurrency.Entities.Banking;
using Concurrency.Repositories.Interfaces.Base;

namespace Concurrency.Repositories.Interfaces
{
    public interface ITransactionRepository: IRepository<Transaction>
    {
    }
}
