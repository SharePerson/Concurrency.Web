﻿using Concurrency.Dto;
using Concurrency.Dto.Base;
using Concurrency.Dto.Enums;
using Concurrency.Entities;
using Concurrency.Entities.Banking;
using Concurrency.Services.Base;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concurrency.Services.Interfaces.Generic
{
    /// <summary>
    /// In order for this implementation class to be used, 
    /// the input type AccountDtoDT must inherit from AccountBase as a requirement
    /// and the input type TransactionStatusDT must be an enum
    /// </summary>
    /// <typeparam name="TransactionStatusDT"></typeparam>
    /// <typeparam name="AccountDtoDT"></typeparam>
    public class BookingGateway<TransactionStatusDT, AccountDtoDT> : Mapper, IBookingGateway<TransactionStatusDT, AccountDtoDT> where AccountDtoDT : AccountBase where TransactionStatusDT : Enum
    {
        private readonly ConcurrencyDbContext dbContext;

        private ILog Log => LogManager.GetLogger(typeof(BookingGateway));

        public BookingGateway(ConcurrencyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<TransactionStatusDT> Deposit(AccountDtoDT account, double amount)
        {
            try
            {
                Account accountToUpdate = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

                if (accountToUpdate == null) return (TransactionStatusDT)(object)TransactionStatus.AccountNotFound;

                //to include the row version in the update query generated by ef core
                dbContext.Entry(accountToUpdate).Property(nameof(accountToUpdate.RowVersion)).OriginalValue = account.RowVersion;

                DateTime operationDate = DateTime.Now;

                accountToUpdate.Balance += amount;
                accountToUpdate.LastTransactionDate = operationDate;

                dbContext.Accounts.Update(accountToUpdate);

                await dbContext.Transactions.AddAsync(new Transaction
                {
                    Amount = amount,
                    Description = $"Branch visit deposit: +{amount}",
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    TransactionDate = operationDate
                });

                await dbContext.SaveChangesAsync();
                return (TransactionStatusDT)(object)TransactionStatus.Success;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //intended fire and forget
                Task.Run(() => Log.Error(ex.Message, ex));

                EntityEntry exEntry = ex.Entries.SingleOrDefault();

                if (exEntry != null)
                {
                    Account clientEntry = exEntry.Entity as Account;

                    if (clientEntry != null)
                    {
                        PropertyValues dbValues = await exEntry.GetDatabaseValuesAsync();

                        if (dbValues == null) return (TransactionStatusDT)(object)TransactionStatus.AccountNotFound;

                        Account dbEntry = dbValues.ToObject() as Account;

                        if (dbEntry != null)
                        {
                            if (dbEntry.Balance != clientEntry.Balance)
                            {
                                account.RowVersion = dbEntry.RowVersion;
                                account.Balance = dbEntry.Balance;
                                return (TransactionStatusDT)(object)TransactionStatus.OutdatedAccount;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //intended fire and forget
                Task.Run(() => Log.Error(ex.Message, ex));
            }

            return (TransactionStatusDT)(object)TransactionStatus.Failure;
        }

        public async Task<AccountDtoDT> GetRandomAccount()
        {
            List<string> guids = new()
            {
                "1CFBBE9E-D9AD-4512-98C7-1AC32C0949F8",
                "10D7E635-51F7-4061-A89D-5B62BEB361F4",
                "E7661426-9171-426B-AA63-5EF958830A8E",
                "35E902FA-034F-4E32-89E5-8F8019906FBD",
                "67675CF8-7518-4551-B775-E89C467D4228"
            };

            Random random = new Random();

            Guid randomeAccountId = Guid.Parse(guids[random.Next(0, 5)]);
            return MapObject<Account, AccountDto>(await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == randomeAccountId)) as AccountDtoDT;
        }

        public async Task<TransactionStatusDT> Transfer(AccountDtoDT fromAccount, AccountDtoDT toAccount, double amount)
        {
            if (fromAccount == null || toAccount == null || amount <= 0) return (TransactionStatusDT)(object)TransactionStatus.BadInput;

            if (fromAccount.Id == toAccount.Id) return (TransactionStatusDT)(object)TransactionStatus.SameAccountRejection;

            Account fromAccountEntity = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == fromAccount.Id);

            if (fromAccountEntity == null) return (TransactionStatusDT)(object)TransactionStatus.FromAccountNotFound;

            if (fromAccountEntity.Balance < amount) return (TransactionStatusDT)(object)TransactionStatus.FromAccountInsufficientFunds;

            dbContext.Entry(fromAccountEntity).Property(nameof(fromAccountEntity.RowVersion)).OriginalValue = fromAccount.RowVersion;

            DateTime operationDate = DateTime.Now;

            fromAccountEntity.Balance -= amount;
            fromAccountEntity.LastTransactionDate = operationDate;

            Account toAccountEntity = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == toAccount.Id);

            if (toAccountEntity == null) return (TransactionStatusDT)(object)TransactionStatus.ToAccountNotFound;

            dbContext.Entry(toAccountEntity).Property(nameof(toAccountEntity.RowVersion)).OriginalValue = toAccount.RowVersion;

            toAccountEntity.Balance += amount;
            toAccountEntity.LastTransactionDate = operationDate;

            try
            {
                dbContext.Accounts.Update(fromAccountEntity);
                dbContext.Accounts.Update(toAccountEntity);

                await dbContext.Transactions.AddAsync(new Transaction
                {
                    Amount = -amount,
                    Description = $"Online transfer to {toAccount.AccountHolderName}: -${amount}",
                    Id = Guid.NewGuid(),
                    AccountId = fromAccountEntity.Id,
                    TransactionDate = operationDate
                });

                await dbContext.Transactions.AddAsync(new Transaction
                {
                    Amount = amount,
                    Description = $"Online transfer from {fromAccount.AccountHolderName}: +${amount}",
                    Id = Guid.NewGuid(),
                    AccountId = toAccount.Id,
                    TransactionDate = operationDate
                });

                await dbContext.SaveChangesAsync();
                return (TransactionStatusDT)(object)TransactionStatus.Success;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //intended fire and forget
                Task.Run(() => Log.Error(ex.Message, ex));

                EntityEntry exEntry = ex.Entries.SingleOrDefault();

                if (exEntry != null)
                {
                    Account clientEntry = exEntry.Entity as Account;

                    if (clientEntry != null)
                    {
                        PropertyValues dbValues = await exEntry.GetDatabaseValuesAsync();

                        if (clientEntry.Id == fromAccount.Id)
                        {
                            if (dbValues == null) return (TransactionStatusDT)(object)TransactionStatus.FromAccountNotFound;

                            Account dbEntry = dbValues.ToObject() as Account;

                            if (dbEntry != null)
                            {
                                if (dbEntry.Balance != clientEntry.Balance)
                                {
                                    fromAccount.RowVersion = dbEntry.RowVersion;
                                    fromAccount.Balance = dbEntry.Balance;
                                    return (TransactionStatusDT)(object)TransactionStatus.OutdatedFromAccount;
                                }
                            }
                        }

                        if (clientEntry.Id == toAccount.Id)
                        {
                            if (dbValues == null) return (TransactionStatusDT)(object)TransactionStatus.ToAccountNotFound;

                            Account dbEntry = dbValues.ToObject() as Account;

                            if (dbEntry != null)
                            {
                                if (dbEntry.Balance != clientEntry.Balance)
                                {
                                    toAccount.RowVersion = dbEntry.RowVersion;
                                    toAccount.Balance = dbEntry.Balance;
                                    return (TransactionStatusDT)(object)TransactionStatus.OutdatedToAccount;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //intended fire and forget because Log.Error is synchronous
                Task.Run(() => Log.Error(ex.Message, ex));
            }

            return (TransactionStatusDT)(object)TransactionStatus.Failure;
        }

        public async Task<TransactionStatusDT> Withdraw(AccountDtoDT account, double amount)
        {
            try
            {
                Account accountToUpdate = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

                if (accountToUpdate == null) return (TransactionStatusDT)(object)TransactionStatus.AccountNotFound;

                if (accountToUpdate.Balance < amount) return (TransactionStatusDT)(object)TransactionStatus.InsufficientFunds;

                dbContext.Entry(accountToUpdate).Property(nameof(accountToUpdate.RowVersion)).OriginalValue = account.RowVersion;

                DateTime operationDate = DateTime.Now;

                accountToUpdate.Balance -= amount;
                accountToUpdate.LastTransactionDate = operationDate;

                dbContext.Accounts.Update(accountToUpdate);

                await dbContext.Transactions.AddAsync(new Transaction
                {
                    Amount = -amount,
                    Description = $"Branch visit withdraw: -{amount}",
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    TransactionDate = operationDate
                });

                await dbContext.SaveChangesAsync();
                return (TransactionStatusDT)(object)TransactionStatus.Success;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //intended fire and forget
                Task.Run(() => Log.Error(ex.Message, ex));
                EntityEntry exEntry = ex.Entries.SingleOrDefault();

                if (exEntry != null)
                {
                    Account clientEntry = exEntry.Entity as Account;

                    if (clientEntry != null)
                    {
                        PropertyValues dbValues = await exEntry.GetDatabaseValuesAsync();

                        if (dbValues == null) return (TransactionStatusDT)(object)TransactionStatus.AccountNotFound;

                        Account dbEntry = dbValues.ToObject() as Account;

                        if (dbEntry != null)
                        {
                            if (dbEntry.Balance != clientEntry.Balance)
                            {
                                account.RowVersion = dbEntry.RowVersion;
                                account.Balance = dbEntry.Balance;
                                return (TransactionStatusDT)(object)TransactionStatus.OutdatedAccount;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //intended fire and forget
                Task.Run(() => Log.Error(ex.Message, ex));
            }

            return (TransactionStatusDT)(object)TransactionStatus.Failure;
        }

        public async ValueTask DisposeAsync()
        {
            await dbContext.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
