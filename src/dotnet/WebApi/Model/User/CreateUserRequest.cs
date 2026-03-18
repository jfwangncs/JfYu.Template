namespace WebApi.Model.User
{
    public class CreateUserRequest
    {
        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string? NickName { get; set; }

        public string? RealName { get; set; }

        public string? Phone { get; set; }
    }
}
