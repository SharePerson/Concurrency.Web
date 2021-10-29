namespace Concurrency.Dto.Enums
{
    public enum TransactionStatus
    {
        Success,
        Failure,
        InsufficientFunds,
        AccountNotFound,
        TicketNotFound,
        TicketAlreadyBooked,
        TicketDatePassed,
        OutdatedTicket,
        OutdatedAccount,
        InvalidBalance,
        FromAccountInsufficientFunds,
        FromAccountNotFound,
        OutdatedFromAccount,
        FromAccountInvalidBalance,
        ToAccountInsufficientFunds,
        ToAccountNotFound,
        OutdatedToAccount,
        ToAccountInvalidBalance,
        SameAccountRejection,
        BadInput
    }
}
