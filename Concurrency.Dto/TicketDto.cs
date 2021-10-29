using System;

namespace Concurrency.Dto
{
    public class TicketDto
    {
        public Guid Id { set; get; }

        public DateTime TicketDate { set; get; }

        public DateTime? ReservationDate { set; get; }

        public Guid? ReservedById { set; get; }

        public bool IsAvailable { set; get; }

        public byte[] RowVersion { set; get; }

        public double Price { set; get; }
    }
}
