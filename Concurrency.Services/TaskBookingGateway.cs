using Concurrency.Dto;
using Concurrency.Dto.Enums;
using Concurrency.Services.Base;
using Concurrency.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Concurrency.Services
{
    public class TaskBookingGateway : Mapper, ITaskBookingGateway
    {
        private readonly IBookingGateway bookingGateway;

        public TaskBookingGateway(IBookingGateway bookingGateway)
        {
            this.bookingGateway = bookingGateway;
        }

        public TransactionStatus Deposit(AccountDto account, double amount)
        {
            try
            {
                Task<TransactionStatus> task = Task.Run(async () => await bookingGateway.Deposit(account, amount));
                task.ConfigureAwait(false);
                task.Wait();
                return task.Result;
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Console.WriteLine("Encountered Aggregate Exception {0}: {1}", e.GetType().Name, e.Message);
                    return true;
                });
            }

            return TransactionStatus.Failure;
        }

        public AccountDto GetRandomAccount()
        {
            try
            {
                Task<AccountDto> task = Task.Run(async () => await bookingGateway.GetRandomAccount());
                task.ConfigureAwait(false);
                return task.GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Console.WriteLine("Encountered Aggregate Exception {0}: {1}", e.GetType().Name, e.Message);
                    return true;
                });
            }

            return null;
        }

        public TransactionStatus Transfer(AccountDto fromAccount, AccountDto toAccount, double amount)
        {
            try
            {
                Task<TransactionStatus> task = Task.Run(async () => await bookingGateway.Transfer(fromAccount, toAccount, amount));
                task.ConfigureAwait(false);
                return task.GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Console.WriteLine("Encountered Aggregate Exception {0}: {1}", e.GetType().Name, e.Message);
                    return true;
                });
            }

            return TransactionStatus.Failure;
        }

        public TransactionStatus Withdraw(AccountDto account, double amount)
        {
            try
            {
                Task<TransactionStatus> task = Task.Run(async () => await bookingGateway.Withdraw(account, amount));
                task.ConfigureAwait(false);
                return task.GetAwaiter().GetResult();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Console.WriteLine("Encountered Aggregate Exception {0}: {1}", e.GetType().Name, e.Message);
                    return true;
                });
            }

            return TransactionStatus.Failure;
        }
    }
}
