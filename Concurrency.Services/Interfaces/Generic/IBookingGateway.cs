using Concurrency.Dto;
using System;
using System.Threading.Tasks;

namespace Concurrency.Services.Interfaces.Generic
{
    public interface IBookingGateway<TransactionResultDataType, AccountDataType> : IAsyncDisposable where AccountDataType : class where TransactionResultDataType : class
    {
        Task<TransactionResultDataType> Withdraw(AccountDataType account, double amount);

        Task<TransactionResultDataType> Deposit(AccountDataType account, double amount);

        Task<TransactionResultDataType> Transfer(AccountDataType fromAccount, AccountDataType toAccount, double amount);

        Task<AccountDataType> GetRandomAccount();

        Task<TicketDto> GetRandomTicket(bool? isAvailable = null);

        Task<TransactionResultDataType> BookTicket(AccountDataType account, TicketDto ticket);

        Task<TransactionResultDataType> UnbookTicket(AccountDataType account, TicketDto ticket);

        Task<AccountDataType> GetTicketOwner(Guid ticketId);
    }
}
