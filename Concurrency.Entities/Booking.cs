using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Concurrency.Entities
{
    public class Booking
    {
        [Key]
        public long Id { set; get; }

        public DateTime BookingDate { set; get; }

        public string BookingUserId { set; get; }

        public int SlotId { set; get; }

        public string Notes { set; get; }

        [Timestamp]
        public byte[] RowVersion { set; get; }

        [ForeignKey(nameof(SlotId))]
        public virtual Slot Slot { set; get; }
    }
}
