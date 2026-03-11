namespace WebApi.Model.Response
{
    public class LoginResponse
    {
        public int Id { get; set; }
        public string AccessToken { get; set; } = null!;
        public int ExpiresIn { get; set; }
        public string Username { get; set; } = null!;
    }
}
