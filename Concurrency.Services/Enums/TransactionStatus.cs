namespace Concurrency.Services.Enums
{
    public enum TransactionStatus
    {
        Success,
        Failure,
        InsufficientFunds,
        AccountNotFound,
        OutdatedAccount,
        InvalidBalance        
    }
}
