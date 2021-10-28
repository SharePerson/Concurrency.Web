using Concurrency.Dto;
using Concurrency.Dto.Enums;
using System;
using System.Threading.Tasks;

namespace Concurrency.Services.Interfaces
{
    public interface IBookingGateway: IAsyncDisposable
    {
        Task<TransactionStatus> Withdraw(AccountDto account, double amount);

        Task<TransactionStatus> Deposit(AccountDto account, double amount);

        Task<TransactionStatus> Transfer(AccountDto fromAccount, AccountDto toAccount, double amount);

        Task<AccountDto> GetRandomAccount();
    }
}
