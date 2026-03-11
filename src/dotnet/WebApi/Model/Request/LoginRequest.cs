using WebApi.Constants;

namespace WebApi.Model.Request
{
    public class LoginRequest
    {
        public string? Phone { get; set; }

        public string? Code { get; set; }


        public string? UserName { get; set; }

        public string? Password { get; set; }

        public PlatformEnum Platform { get; set; } = PlatformEnum.Web;
    }
}
