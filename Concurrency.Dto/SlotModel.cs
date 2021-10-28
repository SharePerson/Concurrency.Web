namespace Concurrency.Dto
{
    public class SlotModel
    {
        public int Id { set; get; }

        public string Name { set; get; }

        public bool IsAvailable { set; get; } = true;

        public byte[] RowVersion { set; get; }
    }
}
