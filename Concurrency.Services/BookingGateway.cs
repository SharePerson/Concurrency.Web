﻿using Concurrency.Dto;
using Concurrency.Dto.Enums;
using Concurrency.Entities;
using Concurrency.Entities.Banking;
using Concurrency.Services.Base;
using Concurrency.Services.Interfaces;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concurrency.Services
{
    public class BookingGateway : Mapper, IBookingGateway
    {
        private readonly ConcurrencyDbContext dbContext;

        private ILog Log => LogManager.GetLogger(typeof(BookingGateway));

        public BookingGateway(ConcurrencyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// implements a branch deposit to an account with optimistic concurrency
        /// </summary>
        /// <param name="account"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<TransactionStatus> Deposit(AccountDto account, double amount)
        {
            Account accountToUpdate = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

            if (accountToUpdate == null) return TransactionStatus.AccountNotFound;

            //to include the row version in the update query generated by ef core
            dbContext.Entry(accountToUpdate).Property(nameof(accountToUpdate.RowVersion)).OriginalValue = account.RowVersion;

            DateTime operationDate = DateTime.Now;

            accountToUpdate.Balance += amount;
            accountToUpdate.LastTransactionDate = operationDate;

            try
            {
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
                return TransactionStatus.Success;
            }
            catch(DbUpdateConcurrencyException ex)
            {
                //intended fire and forget
                Task.Run(() => Log.Error(ex.Message, ex));

                EntityEntry exEntry = ex.Entries.SingleOrDefault();

                if(exEntry != null)
                {
                    Account clientEntry = exEntry.Entity as Account;

                    if(clientEntry != null)
                    {
                        PropertyValues dbValues = await exEntry.GetDatabaseValuesAsync();

                        if (dbValues == null) return TransactionStatus.AccountNotFound;

                        Account dbEntry = dbValues.ToObject() as Account;

                        if(dbEntry != null)
                        {
                            if (dbEntry.Balance != clientEntry.Balance)
                            {
                                account.RowVersion = dbEntry.RowVersion;
                                account.Balance = dbEntry.Balance;
                                return TransactionStatus.OutdatedAccount;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                //intended fire and forget
                Task.Run(() => Log.Error(ex.Message, ex));
            }

            return TransactionStatus.Failure;
        }

        /// <summary>
        /// implements a branch withdraw from an account with optimistic concurrency
        /// </summary>
        /// <param name="account"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<TransactionStatus> Withdraw(AccountDto account, double amount)
        {
            Account accountToUpdate = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

            if (accountToUpdate == null) return TransactionStatus.AccountNotFound;

            if (accountToUpdate.Balance < amount) return TransactionStatus.InsufficientFunds;

            dbContext.Entry(accountToUpdate).Property(nameof(accountToUpdate.RowVersion)).OriginalValue = account.RowVersion;

            DateTime operationDate = DateTime.Now;

            accountToUpdate.Balance -= amount;
            accountToUpdate.LastTransactionDate = operationDate;

            try
            {
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
                return TransactionStatus.Success;
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

                        if (dbValues == null) return TransactionStatus.AccountNotFound;

                        Account dbEntry = dbValues.ToObject() as Account;

                        if (dbEntry != null)
                        {
                            if (dbEntry.Balance != clientEntry.Balance)
                            {
                                account.RowVersion = dbEntry.RowVersion;
                                account.Balance = dbEntry.Balance;
                                return TransactionStatus.OutdatedAccount;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                //intended fire and forget
                Task.Run(() => Log.Error(ex.Message, ex));
            }

            return TransactionStatus.Failure;
        }

        /// <summary>
        /// implements an online transfer between 2 accounts with optimistic concurrency
        /// </summary>
        /// <param name="fromAccount"></param>
        /// <param name="toAccount"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<TransactionStatus> Transfer(AccountDto fromAccount, AccountDto toAccount, double amount)
        {
            if (fromAccount == null || toAccount == null || amount <= 0) return TransactionStatus.BadInput;

            if (fromAccount.Id == toAccount.Id) return TransactionStatus.SameAccountRejection;

            Account fromAccountEntity = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == fromAccount.Id);

            if (fromAccountEntity == null) return TransactionStatus.FromAccountNotFound;

            if (fromAccountEntity.Balance < amount) return TransactionStatus.FromAccountInsufficientFunds;

            dbContext.Entry(fromAccountEntity).Property(nameof(fromAccountEntity.RowVersion)).OriginalValue = fromAccount.RowVersion;

            DateTime operationDate = DateTime.Now;

            fromAccountEntity.Balance -= amount;
            fromAccountEntity.LastTransactionDate = operationDate;

            Account toAccountEntity = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == toAccount.Id);

            if (toAccountEntity == null) return TransactionStatus.ToAccountNotFound;

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
                return TransactionStatus.Success;
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
                            if (dbValues == null) return TransactionStatus.FromAccountNotFound;

                            Account dbEntry = dbValues.ToObject() as Account;

                            if (dbEntry != null)
                            {
                                if (dbEntry.Balance != clientEntry.Balance)
                                {
                                    fromAccount.RowVersion = dbEntry.RowVersion;
                                    fromAccount.Balance = dbEntry.Balance;
                                    return TransactionStatus.OutdatedFromAccount;
                                }
                            }
                        }
                        
                        if(clientEntry.Id == toAccount.Id)
                        {
                            if (dbValues == null) return TransactionStatus.ToAccountNotFound;

                            Account dbEntry = dbValues.ToObject() as Account;

                            if (dbEntry != null)
                            {
                                if (dbEntry.Balance != clientEntry.Balance)
                                {
                                    toAccount.RowVersion = dbEntry.RowVersion;
                                    toAccount.Balance = dbEntry.Balance;
                                    return TransactionStatus.OutdatedToAccount;
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

            return TransactionStatus.Failure;
        }

        public async Task<AccountDto> GetRandomAccount()
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
            return MapObject<Account, AccountDto>(await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == randomeAccountId));
        }

        public async ValueTask DisposeAsync()
        {
            await dbContext.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
