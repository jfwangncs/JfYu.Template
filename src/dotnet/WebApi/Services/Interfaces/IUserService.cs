using JfYu.Data.Model;
using JfYu.Data.Service;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.User;

namespace WebApi.Services.Interfaces
{
    public interface IUserService : IService<User, AppDbContext>
    {
        Task<User?> LoginAsync(LoginRequest login);

        Task<PagedData<User>> GetPagedAsync(QueryRequest query);
    }
}
