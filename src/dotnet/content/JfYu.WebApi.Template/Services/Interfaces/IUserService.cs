using JfYu.Data.Model;
using JfYu.Data.Service;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Model;
using JfYu.WebApi.Template.Model.Request;
using JfYu.WebApi.Template.Model.User;

namespace JfYu.WebApi.Template.Services.Interfaces
{
    public interface IUserService : IService<User, AppDbContext>
    {
        Task<User?> LoginAsync(LoginRequest login);

        Task<PagedData<User>> GetPagedAsync(QueryRequest query);

        Task<bool> UpdateAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

        Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    }
}
