using System;
using System.Threading.Tasks;

namespace Concurrency.Services.Interfaces.Generic
{
    public interface IBookingGateway<TransactionStatusDataType, AccountDataType> : IAsyncDisposable where AccountDataType : class
    {
        Task<TransactionStatusDataType> Withdraw(AccountDataType account, double amount);

        Task<TransactionStatusDataType> Deposit(AccountDataType account, double amount);

        Task<TransactionStatusDataType> Transfer(AccountDataType fromAccount, AccountDataType toAccount, double amount);

        Task<AccountDataType> GetRandomAccount();
    }
}
