using Concurrency.Dto;
using Concurrency.Services.Enums;

namespace Concurrency.Services.Interfaces
{
    public interface ITaskBookingGateway
    {
        TransactionStatus Withdraw(AccountDto account, double amount);

        TransactionStatus Deposit(AccountDto account, double amount);

        TransferStatus Transfer(AccountDto fromAccount, AccountDto toAccount, double amount);

        AccountDto GetRandomAccount();
    }
}
