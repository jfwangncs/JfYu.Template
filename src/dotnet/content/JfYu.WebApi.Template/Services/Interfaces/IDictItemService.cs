using JfYu.Data.Model;
using JfYu.Data.Service;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Model.DictItem;

namespace JfYu.WebApi.Template.Services.Interfaces
{
    public interface IDictItemService : IService<DictItem, AppDbContext>
    {
        Task<PagedData<DictItem>> GetPagedAsync(QueryDictItemRequest query);
        Task<bool> IsCodeDuplicateAsync(int dictTypeId, string code, int? excludeId = null);
    }
}
