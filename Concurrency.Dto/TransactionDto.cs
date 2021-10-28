using System;

namespace Concurrency.Dto
{
    public class TransactionDto
    {
        public Guid Id { set; get; }

        public string Description { set; get; }

        public double Amount { set; get; }

        public Guid AccountId { set; get; }

        public DateTime TransactionDate { set; get; }

        public AccountDto Account { set; get; }
    }
}
