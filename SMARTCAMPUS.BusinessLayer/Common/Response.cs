using SMARTCAMPUS.EntityLayer.DTOs;
using System.Text.Json.Serialization;

namespace SMARTCAMPUS.BusinessLayer.Common
{
    public class Response<T>
    {
        public T Data { get; set; }

        [JsonIgnore]
        public int StatusCode { get; set; }

        public bool IsSuccessful { get; set; }

        public List<string> Errors { get; set; }

        // Static factory methods for successful responses
        public static Response<T> Success(T data, int statusCode)
        {
            return new Response<T> { Data = data, StatusCode = statusCode, IsSuccessful = true };
        }

        public static Response<T> Success(int statusCode)
        {
            T data = default;
            if (typeof(T) == typeof(NoDataDto))
            {
                data = (T)(object)new NoDataDto();
            }

            return new Response<T> { Data = data, StatusCode = statusCode, IsSuccessful = true };
        }

        // Static factory methods for error responses
        public static Response<T> Fail(List<string> errors, int statusCode)
        {
            return new Response<T> { Errors = errors, StatusCode = statusCode, IsSuccessful = false };
        }

        public static Response<T> Fail(string error, int statusCode)
        {
            return new Response<T> { Errors = new List<string> { error }, StatusCode = statusCode, IsSuccessful = false };
        }
    }
}
