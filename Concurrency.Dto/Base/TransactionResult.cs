using Concurrency.Dto.Enums;
using System;

namespace Concurrency.Dto.Base
{
    public class TransactionResult<T> where T: class
    {
        public T Data { set; get; }

        public double TransferedAmount { set; get; }

        //public Exception Exception { set; get; }

        public bool IsFaulted { set; get; }

        public TransactionStatus TransactionStatus { set; get; }
    }
}
