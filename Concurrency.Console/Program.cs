using Concurrency.Dto;
using Concurrency.Dto.Base;
using Concurrency.Dto.Enums;
using Concurrency.Entities;
using Concurrency.Services.Factories;
using Concurrency.Services.Interfaces.Generic;
using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Concurrency.Demo
{
    class Program: IDesignTimeDbContextFactory<ConcurrencyDbContext>
    {
        private const int MAX_OPERATIONS = 1000;
        private const int MAX_PARALLEL_OPERATIONS = 10;
        private const int MIN_DEPOSIT = 100;
        private const int MAX_DEPOSIT = 1000;
        private const int MIN_WITHDRAW = 200;
        private const int MAX_WITHDRAW = 500;
        private const int MIN_TRANSFER = 500;
        private const int MAX_TRANSFER = 700;

        static void Main(string[] args)
        {
            Program program = new();
            BookingGatewayFactory<IBookingGateway<TransactionResult<AccountDto>, AccountDto>> gatewayFactory = new();
            using (ConcurrencyDbContext dbContext = program.CreateDbContext(null))
            {
                dbContext.Database.Migrate();
            }

            #region Banking with Async Gateway
            ThreadPool.SetMinThreads(8, 8);
            ThreadPool.SetMaxThreads(32767, 1000);

            CancellationTokenSource cancellationTokenSource = new();

            #region Init Gateway in a Broadcast
            var initGatewayBlock = new BroadcastBlock<IBookingGateway<TransactionResult<AccountDto>, AccountDto>>((gateway) =>
            {
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
                BookingGatewayFactory<IBookingGateway<TransactionResult<AccountDto>, AccountDto>> gatewayFactory = new();
                return gatewayFactory.CreateSqlite();
            });
            #endregion

            #region Deposit to Account Stage
            var depositFromAccountBlock = new TransformBlock<IBookingGateway<TransactionResult<AccountDto>, AccountDto>, TransactionResult<AccountDto>>(
                async (bookingGateway) =>
                {
                    Random random = new();
                    AccountDto fromAccount = null;
                    double randomFromAmount = random.Next(MIN_DEPOSIT, MAX_DEPOSIT);

                    try
                    {
                        fromAccount = await bookingGateway.GetRandomAccount();
                        return await bookingGateway.Deposit(fromAccount, randomFromAmount);
                    }
                    catch (Exception ex)
                    {
                        cancellationTokenSource.Cancel();
                        return new TransactionResult<AccountDto>
                        {
                            Data = fromAccount,
                            Exception = ex,
                            TransactionStatus = TransactionStatus.Failure,
                            TransferedAmount = randomFromAmount
                        };
                    }
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MAX_PARALLEL_OPERATIONS,
                    CancellationToken = cancellationTokenSource.Token
                });

            var writeDepositTransactionBlock = new ActionBlock<TransactionResult<AccountDto>>(result =>
            {
                Console.WriteLine($"Transaction deposit to account {result.Data?.AccountHolderName ?? "NULL"} with amount {result.TransferedAmount} result: {(result.Exception != null ? result.Exception.ToString() : result.TransactionStatus)}");
            });

            initGatewayBlock.LinkTo(depositFromAccountBlock, new DataflowLinkOptions { PropagateCompletion = true });
            depositFromAccountBlock.LinkTo(writeDepositTransactionBlock, new DataflowLinkOptions { PropagateCompletion = true });
            #endregion

            #region Withdraw from Account Stage
            var withdrawFromAccountBlock = new TransformBlock<IBookingGateway<TransactionResult<AccountDto>, AccountDto>, TransactionResult<AccountDto>>(
                async (bookingGateway) =>
                {
                    Random random = new();
                    double randomAmount = random.Next(MIN_WITHDRAW, MAX_WITHDRAW);
                    AccountDto fromAccount = null;

                    try
                    {
                        fromAccount = await bookingGateway.GetRandomAccount();
                        return await bookingGateway.Withdraw(fromAccount, randomAmount);
                    }
                    catch (Exception ex)
                    {
                        cancellationTokenSource.Cancel();
                        return new TransactionResult<AccountDto>
                        {
                            Data = fromAccount,
                            Exception = ex,
                            TransactionStatus = TransactionStatus.Failure,
                            TransferedAmount = randomAmount
                        };
                    }
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MAX_PARALLEL_OPERATIONS,
                    CancellationToken = cancellationTokenSource.Token
                });

            var writeWithdrawTransactionBlock = new ActionBlock<TransactionResult<AccountDto>>(result =>
            {
                Console.WriteLine($"Transaction withdraw from account {result.Data?.AccountHolderName ?? "NULL"} with amount {result.TransferedAmount} result: {(result.Exception != null ? result.Exception.ToString() : result.TransactionStatus)}");
            });

            initGatewayBlock.LinkTo(withdrawFromAccountBlock, new DataflowLinkOptions { PropagateCompletion = true });
            withdrawFromAccountBlock.LinkTo(writeWithdrawTransactionBlock, new DataflowLinkOptions { PropagateCompletion = true });
            #endregion

            #region Transfer between Accounts Stags
            var transferBlock = new TransformBlock<IBookingGateway<TransactionResult<AccountDto>, AccountDto>, TransactionResult<AccountDto>>(
                async (bookingGateway) =>
                {
                    AccountDto fromAccount = null;
                    AccountDto toAccount = null;
                    Random random = new();
                    double randomAmount = random.Next(MIN_TRANSFER, MAX_TRANSFER);

                    try
                    {
                        fromAccount = await bookingGateway.GetRandomAccount();
                        toAccount = await bookingGateway.GetRandomAccount();

                        return await bookingGateway.Transfer(fromAccount, toAccount, randomAmount);
                    }
                    catch (Exception ex)
                    {
                        cancellationTokenSource.Cancel();
                        return new TransactionResult<AccountDto>
                        {
                            Data = fromAccount,
                            Exception = ex,
                            TransactionStatus = TransactionStatus.Failure,
                            TransferedAmount = randomAmount
                        }; ;
                    }
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MAX_PARALLEL_OPERATIONS,
                    CancellationToken = cancellationTokenSource.Token
                });

            var writeTransferStatusBlock = new ActionBlock<TransactionResult<AccountDto>>(result =>
            {
                Console.WriteLine($"Transfer status from account {result.Data?.AccountHolderName ?? "NULL"} with amount {result.TransferedAmount} result: {(result.Exception != null ? result.Exception.ToString() : result.TransactionStatus)}");
            });

            initGatewayBlock.LinkTo(transferBlock, new DataflowLinkOptions { PropagateCompletion = true });
            transferBlock.LinkTo(writeTransferStatusBlock, new DataflowLinkOptions { PropagateCompletion = true });
            #endregion

            for (int i = 0; i < MAX_OPERATIONS; i++)
            {
                initGatewayBlock.Post(null);
            }

            Console.WriteLine($"Finished broadcasting {MAX_OPERATIONS} operations...");

            try
            {
                initGatewayBlock.Complete();

                Task.WhenAll(writeDepositTransactionBlock.Completion,
                    writeWithdrawTransactionBlock.Completion,
                    writeTransferStatusBlock.Completion)
                    .Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Console.WriteLine("Encountered Aggregate Exception {0}: {1}", e.GetType().Name, e.Message);
                    return true;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Encountered Main Exception {0}: {1}", ex.GetType().Name, ex.Message);
            }
            #endregion
        }

        public ConcurrencyDbContext CreateDbContext(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false)
            .SetBasePath(Directory.GetCurrentDirectory());
            var configuration = builder.Build();

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddDbContext<ConcurrencyDbContext>(options => options.UseSqlite(configuration.GetConnectionString("SqliteConnection"), builder =>
                {
                    builder.MigrationsAssembly("Concurrency.Migrations.Sqlite");
                }), ServiceLifetime.Transient)
                .BuildServiceProvider();

            return serviceProvider.GetRequiredService<ConcurrencyDbContext>();
        }
    }
}
