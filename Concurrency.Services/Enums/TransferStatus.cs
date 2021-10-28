namespace Concurrency.Services.Enums
{
    public enum TransferStatus
    {
        Success,
        Failure,
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
