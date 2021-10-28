﻿using Concurrency.Dto;
using Concurrency.Entities.Banking;
using Concurrency.Repositories.Interfaces;
using Concurrency.Services.Base;
using Concurrency.Services.Enums;
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
    public class UnitBookingGateway : Mapper, IBookingGateway
    {
        private readonly IAccountRepository accountRepository;
        private readonly ITransactionRepository transactionRepository;
        private readonly IUnitOfWork unitOfWork;

        private ILog Log => LogManager.GetLogger(typeof(BookingGateway));

        public UnitBookingGateway(IAccountRepository accountRepository, ITransactionRepository transactionRepository, IUnitOfWork unitOfWork)
        {
            this.accountRepository = accountRepository;
            this.transactionRepository = transactionRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<TransactionStatus> Deposit(AccountDto account, double amount)
        {
            Account accountToUpdate = await accountRepository.Get(a => a.Id == account.Id);

            if (accountToUpdate == null) return TransactionStatus.AccountNotFound;

            //to include the row version in the update query generated by ef core
            accountRepository.SetOriginalValue(accountToUpdate, nameof(accountToUpdate.RowVersion), account.RowVersion);

            DateTime operationDate = DateTime.Now;

            accountToUpdate.Balance += amount;
            accountToUpdate.LastTransactionDate = operationDate;

            try
            {
                accountRepository.Update(accountToUpdate);

                await transactionRepository.Add(new Transaction
                {
                    Amount = amount,
                    Description = $"Branch visit deposit: +{amount}",
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    TransactionDate = operationDate
                });

                await unitOfWork.Commit();
                return TransactionStatus.Success;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Log.Error(ex.Message, ex);
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
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }

            return TransactionStatus.Failure;
        }

        public async ValueTask DisposeAsync()
        {
            await accountRepository.DbContext.DisposeAsync();
            await transactionRepository.DbContext.DisposeAsync();
            await unitOfWork.DbContext.DisposeAsync();
            GC.SuppressFinalize(this);
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
            return MapObject<Account, AccountDto>(await accountRepository.Get(a => a.Id == randomeAccountId));
        }

        public async Task<TransferStatus> Transfer(AccountDto fromAccount, AccountDto toAccount, double amount)
        {
            if (fromAccount == null || toAccount == null || amount <= 0) return TransferStatus.BadInput;

            if (fromAccount.Id == toAccount.Id) return TransferStatus.SameAccountRejection;

            Account fromAccountEntity = await accountRepository.Get(a => a.Id == fromAccount.Id);

            if (fromAccountEntity == null) return TransferStatus.FromAccountNotFound;

            if (fromAccountEntity.Balance < amount) return TransferStatus.FromAccountInsufficientFunds;

            accountRepository.SetOriginalValue(fromAccountEntity, nameof(fromAccountEntity.RowVersion), fromAccount.RowVersion);

            DateTime operationDate = DateTime.Now;

            fromAccountEntity.Balance -= amount;
            fromAccountEntity.LastTransactionDate = operationDate;

            Account toAccountEntity = await accountRepository.Get(a => a.Id == toAccount.Id);

            if (toAccountEntity == null) return TransferStatus.ToAccountNotFound;

            accountRepository.SetOriginalValue(toAccountEntity, nameof(toAccountEntity.RowVersion), toAccount.RowVersion);

            toAccountEntity.Balance += amount;
            toAccountEntity.LastTransactionDate = operationDate;

            try
            {
                accountRepository.Update(fromAccountEntity);
                accountRepository.Update(toAccountEntity);

                await transactionRepository.Add(new Transaction
                {
                    Amount = -amount,
                    Description = $"Online transfer to ${toAccount.AccountHolderName}: -{amount}",
                    Id = Guid.NewGuid(),
                    AccountId = fromAccountEntity.Id,
                    TransactionDate = operationDate
                });

                await transactionRepository.Add(new Transaction
                {
                    Amount = amount,
                    Description = $"Online transfer from ${fromAccount.AccountHolderName}: +{amount}",
                    Id = Guid.NewGuid(),
                    AccountId = toAccount.Id,
                    TransactionDate = operationDate
                });

                await unitOfWork.Commit();
                return TransferStatus.Success;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Log.Error(ex.Message, ex);
                EntityEntry exEntry = ex.Entries.SingleOrDefault();

                if (exEntry != null)
                {
                    Account clientEntry = exEntry.Entity as Account;

                    if (clientEntry != null)
                    {
                        PropertyValues dbValues = await exEntry.GetDatabaseValuesAsync();

                        if (clientEntry.Id == fromAccount.Id)
                        {
                            if (dbValues == null) return TransferStatus.FromAccountNotFound;

                            Account dbEntry = dbValues.ToObject() as Account;

                            if (dbEntry != null)
                            {
                                if (dbEntry.Balance != clientEntry.Balance)
                                {
                                    fromAccount.RowVersion = dbEntry.RowVersion;
                                    fromAccount.Balance = dbEntry.Balance;
                                    return TransferStatus.OutdatedFromAccount;
                                }
                            }
                        }

                        if (clientEntry.Id == toAccount.Id)
                        {
                            if (dbValues == null) return TransferStatus.ToAccountNotFound;

                            Account dbEntry = dbValues.ToObject() as Account;

                            if (dbEntry != null)
                            {
                                if (dbEntry.Balance != clientEntry.Balance)
                                {
                                    toAccount.RowVersion = dbEntry.RowVersion;
                                    toAccount.Balance = dbEntry.Balance;
                                    return TransferStatus.OutdatedToAccount;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }

            return TransferStatus.Failure;
        }

        public async Task<TransactionStatus> Withdraw(AccountDto account, double amount)
        {
            Account accountToUpdate = await accountRepository.Get(a => a.Id == account.Id);

            if (accountToUpdate == null) return TransactionStatus.AccountNotFound;

            if (accountToUpdate.Balance < amount) return TransactionStatus.InsufficientFunds;

            accountRepository.SetOriginalValue(accountToUpdate, nameof(accountToUpdate.RowVersion), account.RowVersion);

            DateTime operationDate = DateTime.Now;

            accountToUpdate.Balance -= amount;
            accountToUpdate.LastTransactionDate = operationDate;

            try
            {
                accountRepository.Update(accountToUpdate);

                await transactionRepository.Add(new Transaction
                {
                    Amount = -amount,
                    Description = $"Branch visit withdraw: -{amount}",
                    Id = Guid.NewGuid(),
                    AccountId = account.Id,
                    TransactionDate = operationDate
                });

                await unitOfWork.Commit();
                return TransactionStatus.Success;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Log.Error(ex.Message, ex);
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
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }

            return TransactionStatus.Failure;
        }
    }
}
