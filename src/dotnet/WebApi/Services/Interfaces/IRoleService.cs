using JfYu.Data.Service;
using WebApi.Entity;
using WebApi.Model;
using WebApi.Model.Request;

namespace WebApi.Services.Interfaces
{
  public interface IRoleService : IService<Role, AppDbContext>
  {
    Task<PagedResult<Role>> GetPagedAsync(QueryRequest query);
  }
}
