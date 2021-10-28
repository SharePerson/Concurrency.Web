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
    public class BookingGateway<TransactionResultDT, AccountDtoDT> : Mapper, IBookingGateway<TransactionResultDT, AccountDtoDT> where AccountDtoDT : AccountBase where TransactionResultDT : TransactionResult<AccountDtoDT>
    {
        private readonly ConcurrencyDbContext dbContext;

        private ILog Log => LogManager.GetLogger(typeof(BookingGateway));

        public BookingGateway(ConcurrencyDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<TransactionResultDT> Deposit(AccountDtoDT account, double amount)
        {
            TransactionResult<AccountDtoDT> transactionResult = new()
            {
                Data = account,
                TransferedAmount = amount
            };

            try
            {
                Account accountToUpdate = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

                if (accountToUpdate == null)
                {
                    transactionResult.TransactionStatus = TransactionStatus.AccountNotFound;
                    return transactionResult as TransactionResultDT;
                }

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

                transactionResult.TransactionStatus = TransactionStatus.Success;
                return transactionResult as TransactionResultDT;
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
            }
            catch (Exception ex)
            {
                //intended fire and forget
                Task.Run(() => Log.Error(ex.Message, ex));
            }

            transactionResult.TransactionStatus = TransactionStatus.Failure;
            return transactionResult as TransactionResultDT;
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

        public async Task<TransactionResultDT> Transfer(AccountDtoDT fromAccount, AccountDtoDT toAccount, double amount)
        {
            TransactionResult<AccountDtoDT> transactionResult = new()
            {
                Data = fromAccount,
                TransferedAmount = amount
            };

            if (fromAccount == null || toAccount == null || amount <= 0)
            {
                transactionResult.TransactionStatus = TransactionStatus.BadInput;
                return transactionResult as TransactionResultDT;
            }

            if (fromAccount.Id == toAccount.Id)
            {
                transactionResult.TransactionStatus = TransactionStatus.SameAccountRejection;
                return transactionResult as TransactionResultDT;
            }

            Account fromAccountEntity = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == fromAccount.Id);

            if (fromAccountEntity == null)
            {
                transactionResult.TransactionStatus = TransactionStatus.FromAccountNotFound;
                return transactionResult as TransactionResultDT;
            }

            if (fromAccountEntity.Balance < amount)
            {
                transactionResult.TransactionStatus = TransactionStatus.FromAccountInsufficientFunds;
                return transactionResult as TransactionResultDT;
            }

            dbContext.Entry(fromAccountEntity).Property(nameof(fromAccountEntity.RowVersion)).OriginalValue = fromAccount.RowVersion;

            DateTime operationDate = DateTime.Now;

            fromAccountEntity.Balance -= amount;
            fromAccountEntity.LastTransactionDate = operationDate;

            Account toAccountEntity = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == toAccount.Id);

            if (toAccountEntity == null)
            {
                transactionResult.TransactionStatus = TransactionStatus.ToAccountNotFound;
                return transactionResult as TransactionResultDT;
            }

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

                transactionResult.TransactionStatus = TransactionStatus.Success;
                return transactionResult as TransactionResultDT;
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
                            if (dbValues == null)
                            {
                                transactionResult.TransactionStatus = TransactionStatus.FromAccountNotFound;
                                return transactionResult as TransactionResultDT;
                            }

                            Account dbEntry = dbValues.ToObject() as Account;

                            if (dbEntry != null)
                            {
                                if (dbEntry.Balance != clientEntry.Balance)
                                {
                                    fromAccount.RowVersion = dbEntry.RowVersion;
                                    fromAccount.Balance = dbEntry.Balance;

                                    transactionResult.TransactionStatus = TransactionStatus.OutdatedFromAccount;
                                    return transactionResult as TransactionResultDT;
                                }
                            }
                        }

                        if (clientEntry.Id == toAccount.Id)
                        {
                            if (dbValues == null)
                            {
                                transactionResult.TransactionStatus = TransactionStatus.ToAccountNotFound;
                                return transactionResult as TransactionResultDT;
                            }

                            Account dbEntry = dbValues.ToObject() as Account;

                            if (dbEntry != null)
                            {
                                if (dbEntry.Balance != clientEntry.Balance)
                                {
                                    toAccount.RowVersion = dbEntry.RowVersion;
                                    toAccount.Balance = dbEntry.Balance;
                                    transactionResult.TransactionStatus = TransactionStatus.OutdatedToAccount;
                                    return transactionResult as TransactionResultDT;
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

            transactionResult.TransactionStatus = TransactionStatus.Failure;
            return transactionResult as TransactionResultDT;
        }

        public async Task<TransactionResultDT> Withdraw(AccountDtoDT account, double amount)
        {
            TransactionResult<AccountDtoDT> transactionResult = new()
            {
                Data = account,
                TransferedAmount = amount
            };

            try
            {
                Account accountToUpdate = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);

                if (accountToUpdate == null)
                {
                    transactionResult.TransactionStatus = TransactionStatus.AccountNotFound;
                    return transactionResult as TransactionResultDT;
                }

                if (accountToUpdate.Balance < amount)
                {
                    transactionResult.TransactionStatus = TransactionStatus.InsufficientFunds;
                    return transactionResult as TransactionResultDT;
                }

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
                transactionResult.TransactionStatus = TransactionStatus.Success;
                return transactionResult as TransactionResultDT;
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
            }
            catch (Exception ex)
            {
                //intended fire and forget
                Task.Run(() => Log.Error(ex.Message, ex));
            }

            transactionResult.TransactionStatus = TransactionStatus.Failure;
            return transactionResult as TransactionResultDT;
        }

        public async ValueTask DisposeAsync()
        {
            await dbContext.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
