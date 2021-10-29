using Concurrency.Dto.Base;
using Concurrency.Dto.Enums;
using Concurrency.Entities.Banking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq;
using System.Threading.Tasks;

namespace Concurrency.Services.Base
{
    public abstract class BookingGatewayBase<TransactionResultDT, AccountDtoDT> where AccountDtoDT : AccountBase where TransactionResultDT : TransactionResult<AccountDtoDT>
    {
        protected async Task<TransactionResult<AccountDtoDT>> HandleAccountConcurrencyErrors(TransactionResult<AccountDtoDT> transactionResult, DbUpdateConcurrencyException ex, AccountDtoDT account)
        {
            transactionResult.IsFaulted = true;

            EntityEntry exEntry = ex.Entries.SingleOrDefault();

            if (exEntry != null)
            {
                Account clientEntry = exEntry.Entity as Account;

                if (clientEntry != null)
                {
                    PropertyValues dbValues = await exEntry.GetDatabaseValuesAsync();

                    if (dbValues == null)
                    {
                        transactionResult.TransactionStatus = TransactionStatus.AccountNotFound;
                        return transactionResult as TransactionResultDT;
                    }

                    Account dbEntry = dbValues.ToObject() as Account;

                    if (dbEntry != null)
                    {
                        if (dbEntry.Balance != clientEntry.Balance)
                        {
                            account.RowVersion = dbEntry.RowVersion;
                            account.Balance = dbEntry.Balance;
                            transactionResult.TransactionStatus = TransactionStatus.OutdatedAccount;
                            return transactionResult as TransactionResultDT;
                        }
                    }
                }
            }

            return transactionResult;
        }
    }
}
