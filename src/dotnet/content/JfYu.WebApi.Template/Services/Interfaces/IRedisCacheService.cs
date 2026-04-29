using JfYu.WebApi.Template.Model.RedisCache;

namespace JfYu.WebApi.Template.Services.Interfaces
{
    public interface IRedisCacheService
    {
        Task<List<RedisCacheItemResponse>> GetAllAsync();
        Task DeleteAsync(List<string> keys);
    }
}
