namespace Concurrency.Dto.Responses
{
    public class OperationResponse<DataType> 
    {
        public DataType Data { set; get; }
        public string Status { set; get; }
        public string Error { set; get; }
    }
}
