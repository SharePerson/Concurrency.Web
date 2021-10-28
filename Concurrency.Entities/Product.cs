using System.ComponentModel.DataAnnotations;

namespace Concurrency.Entities
{
    public class Product
    {
        [Key]
        public int Id { set; get; }

        [Required]
        public string Name { set; get; }

        public string Description { set; get; }

        public double Price { set; get; }

        [Timestamp]
        public byte[] RowVersion { set; get; }
    }
}
