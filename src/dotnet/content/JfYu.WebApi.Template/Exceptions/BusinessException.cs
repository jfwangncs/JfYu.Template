using System.Net;
using JfYu.WebApi.Template.Constants;

namespace JfYu.WebApi.Template.Exceptions
{
    public class BusinessException(ErrorCode errorCode, HttpStatusCode statusCode = HttpStatusCode.BadRequest) : Exception()
    {
        public ErrorCode ErrorCode { get; } = errorCode;
        public HttpStatusCode StatusCode { get; } = statusCode;
    }
}
