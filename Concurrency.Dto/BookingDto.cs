using System;

namespace Concurrency.Dto
{
    public class BookingDto
    {
        public long Id { set; get; }

        public DateTime BookingDate { set; get; }

        public string BookingUserId { set; get; }

        public int SlotId { set; get; }

        public string Notes { set; get; }

        public byte[] RowVersion { set; get; }

        public SlotModel Slot { set; get; }
    }
}
