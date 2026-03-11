using JfYu.Data.Model;
using JfYu.Data.Service;
using WebApi.Entity;
using WebApi.Model.Request;
using WebApi.Model.Response;

namespace WebApi.Services.Interfaces
{
    public interface IUserService : IService<User, AppDbContext>
    {
        Task<User?> LoginAsync(LoginRequest login);

        Task<PagedData<UserResponse>> GetPagedAsync(QueryRequest query);
    }
}
