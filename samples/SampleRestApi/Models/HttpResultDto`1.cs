namespace SampleRestApi.Models
{
    public class HttpResultDto<T>
    {
        public HttpResultDto()
        {
        }

        public HttpResultDto(T result)
        {
            Result = result;
            Success = true;
        }

        public T Result { get; set; }

        public bool Success { get; set; }
    }
}