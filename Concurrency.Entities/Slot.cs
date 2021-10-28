using System.ComponentModel.DataAnnotations;

namespace Concurrency.Entities
{
    public class Slot
    {
        [Key]
        public int Id { set; get; }

        [Required]
        public string Name { set; get; }

        public bool IsAvailable { set; get; } = true;

        [Timestamp]
        public byte[] RowVersion { set; get; }
    }
}
