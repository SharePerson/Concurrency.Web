using System;
using System.Collections.Generic;

namespace Concurrency.Dto
{
    public class AccountDto
    {
        public Guid Id { set; get; }

        public string AccountHolderName { set; get; }

        public double Balance { set; get; }

        public DateTime? LastTransactionDate { set; get; }

        public byte[] RowVersion { set; get; }
    }
}
