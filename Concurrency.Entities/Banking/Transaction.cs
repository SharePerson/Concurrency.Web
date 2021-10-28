using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concurrency.Entities.Banking
{
    public class Transaction
    {
        [Key]
        public Guid Id { set; get; }

        public string Description { set; get; }

        public double Amount { set; get; }

        public Guid AccountId { set; get; }

        public DateTime TransactionDate { set; get; }

        [ForeignKey(nameof(AccountId))]
        public virtual Account Account { set; get; }
    }
}
