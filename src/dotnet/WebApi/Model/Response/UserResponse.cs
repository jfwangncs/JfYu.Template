using WebApi.Constants;

namespace WebApi.Model.Response
{
    public class UserResponse
    {
        public string? SessionKey { get; set; }
        
        public string? OpenId { get; set; }
         
        public string? UnionId { get; set; }
         
        public string? NickName { get; set; }
         
        public string? RealName { get; set; }
         
        public string? Avatar { get; set; }
         
        public Gender? Gender { get; set; }
         
        public string? Province { get; set; }
         
        public string? City { get; set; }
         
        public string? Country { get; set; }
         
        public string? Address { get; set; }
         
        public string? Phone { get; set; }

        public DateTime LastLoginTime { get; set; }
        public DateTime CreatedTime { get; set; }
        public ICollection<RoleResponse> Roles { get; set; } = [];

    }
}
