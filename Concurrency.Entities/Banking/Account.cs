using System;
using System.ComponentModel.DataAnnotations;

namespace Concurrency.Entities.Banking
{
    public class Account
    {
        [Key]
        public Guid Id { set; get; }

        public string AccountHolderName { set; get; }

        public double Balance { set; get; }

        public DateTime? LastTransactionDate { set; get; }

        [Timestamp]
        public byte[] RowVersion { set; get; }
    }
}
