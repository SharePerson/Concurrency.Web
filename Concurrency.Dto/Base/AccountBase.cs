using System;

namespace Concurrency.Dto.Base
{
    public abstract class AccountBase
    {
        public Guid Id { set; get; }

        public string AccountHolderName { set; get; }

        public double Balance { set; get; }

        public DateTime? LastTransactionDate { set; get; }

        public byte[] RowVersion { set; get; }
    }
}
