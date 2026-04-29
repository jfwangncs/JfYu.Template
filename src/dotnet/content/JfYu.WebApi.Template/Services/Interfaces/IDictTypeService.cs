using JfYu.Data.Model;
using JfYu.Data.Service;
using JfYu.WebApi.Template.Entity;
using JfYu.WebApi.Template.Model.DictType;

namespace JfYu.WebApi.Template.Services.Interfaces
{
    public interface IDictTypeService : IService<DictType, AppDbContext>
    {
        Task<PagedData<DictType>> GetPagedAsync(QueryDictTypeRequest query);
        Task<bool> IsCodeDuplicateAsync(string code, int? excludeId = null);
        Task<DictType?> GetByIdWithItemsAsync(int id);
        Task<List<DictType>> GetAllWithItemsAsync();
    }
}
