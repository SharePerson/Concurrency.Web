using Concurrency.Dto;
using Concurrency.Dto.Enums;

namespace Concurrency.Services.Interfaces
{
    public interface ITaskBookingGateway
    {
        TransactionStatus Withdraw(AccountDto account, double amount);

        TransactionStatus Deposit(AccountDto account, double amount);

        TransactionStatus Transfer(AccountDto fromAccount, AccountDto toAccount, double amount);

        AccountDto GetRandomAccount();
    }
}
