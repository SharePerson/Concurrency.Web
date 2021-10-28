using Concurrency.Dto;
using Concurrency.Dto.Enums;
using Concurrency.Services.Factories;
using Concurrency.Services.Interfaces;
using Concurrency.Services.Interfaces.Generic;
using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Concurrency.Demo
{
    class Program
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
            #region Banking with Async Gateway
            ThreadPool.SetMinThreads(8, 8);
            ThreadPool.SetMaxThreads(32767, 1000);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            #region Init Gateway in a Broadcast
            var initGatewayBlock = new BroadcastBlock<IBookingGateway<TransactionStatus, AccountDto>>((gateway) =>
            {
                var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
                XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
                return BookingGatewayFactory.CreateSqlite<IBookingGateway<TransactionStatus, AccountDto>>();
            });
            #endregion

            #region Deposit to Account Stage
            var depositFromAccountBlock = new TransformBlock<IBookingGateway<TransactionStatus, AccountDto>, TransactionStatus>(
                async (bookingGateway) =>
                {
                    try
                    {
                        Random random = new();
                        double randomFromAmount = random.Next(MIN_DEPOSIT, MAX_DEPOSIT);
                        AccountDto fromAccount = await bookingGateway.GetRandomAccount();
                        return await bookingGateway.Deposit(fromAccount, randomFromAmount);
                    }
                    catch
                    {
                        cancellationTokenSource.Cancel();
                        return TransactionStatus.Failure;
                    }
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MAX_PARALLEL_OPERATIONS,
                    CancellationToken = cancellationTokenSource.Token
                });

            var writeDepositTransactionBlock = new ActionBlock<TransactionStatus>(tStatus =>
            {
                Console.WriteLine($"Transaction deposit to account result: {tStatus}");
            });

            initGatewayBlock.LinkTo(depositFromAccountBlock, new DataflowLinkOptions { PropagateCompletion = true });
            depositFromAccountBlock.LinkTo(writeDepositTransactionBlock, new DataflowLinkOptions { PropagateCompletion = true });
            #endregion

            #region Withdraw from Account Stage
            var withdrawFromAccountBlock = new TransformBlock<IBookingGateway<TransactionStatus, AccountDto>, TransactionStatus>(
                async (bookingGateway) =>
                {
                    try
                    {
                        Random random = new();
                        double randomAmount = random.Next(MIN_WITHDRAW, MAX_WITHDRAW);
                        AccountDto fromAccount = await bookingGateway.GetRandomAccount();
                        return await bookingGateway.Withdraw(fromAccount, randomAmount);
                    }
                    catch
                    {
                        cancellationTokenSource.Cancel();
                        return TransactionStatus.Failure;
                    }
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MAX_PARALLEL_OPERATIONS,
                    CancellationToken = cancellationTokenSource.Token
                });

            var writeWithdrawTransactionBlock = new ActionBlock<TransactionStatus>(tStatus => Console.WriteLine($"Transaction withdraw from account result: {tStatus}"));
            initGatewayBlock.LinkTo(withdrawFromAccountBlock, new DataflowLinkOptions { PropagateCompletion = true });
            withdrawFromAccountBlock.LinkTo(writeWithdrawTransactionBlock, new DataflowLinkOptions { PropagateCompletion = true });
            #endregion

            #region Transfer between Accounts Stags
            var transferBlock = new TransformBlock<IBookingGateway<TransactionStatus, AccountDto>, TransactionStatus>(
                async (bookingGateway) =>
                {
                    try
                    {
                        Random random = new();
                        double randomAmount = random.Next(MIN_TRANSFER, MAX_TRANSFER);
                        AccountDto fromAccount = await bookingGateway.GetRandomAccount();
                        AccountDto toAccount = await bookingGateway.GetRandomAccount();

                        return await bookingGateway.Transfer(fromAccount, toAccount, randomAmount);
                    }
                    catch
                    {
                        cancellationTokenSource.Cancel();
                        return TransactionStatus.Failure;
                    }
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = MAX_PARALLEL_OPERATIONS,
                    CancellationToken = cancellationTokenSource.Token
                });

            var writeTransferStatusBlock = new ActionBlock<TransactionStatus>(tStatus => Console.WriteLine($"Transfer status between 2 accounts result: {tStatus}"));
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
    }
}
