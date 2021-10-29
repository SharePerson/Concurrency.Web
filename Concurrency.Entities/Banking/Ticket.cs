using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concurrency.Entities.Banking
{
    public class Ticket
    {
        [Key]
        public Guid Id { set; get; }

        public DateTime TicketDate { set; get; }

        public DateTime? ReservationDate { set; get; }

        public Guid? ReservedById { set; get; }

        public bool IsAvailable { set; get; }

        [Timestamp]
        public byte[] RowVersion { set; get; }

        [ForeignKey(nameof(ReservedById))]
        public virtual Account Account { set; get; }
    }
}
