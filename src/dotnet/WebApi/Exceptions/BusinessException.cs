using System.Net;
using WebApi.Constants;

namespace WebApi.Exceptions
{
    public class BusinessException(ErrorCode errorCode, HttpStatusCode statusCode = HttpStatusCode.BadRequest) : Exception()
    {
        public ErrorCode ErrorCode { get; } = errorCode;
        public HttpStatusCode StatusCode { get; } = statusCode;
    }
}
