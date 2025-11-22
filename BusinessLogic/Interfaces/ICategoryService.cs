using DataLayer.Entities;
using DataLayer.Interfaces;

namespace BusinessLogic.Interfaces;

public interface ICategoryService
{
    Task<Category?> GetByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task<IEnumerable<Category>> GetByUserGuidAsync(string userGuid);
    Task<Category?> GetByNameAsync(string userGuid, string name);
    Task<Category> CreateAsync(Category category);
    Task<Category?> UpdateAsync(int id, Category category);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteWithItemsAsync(int id);
    Task<int> GetItemsCountAsync(int categoryId);
    Task<bool> IsCategoryUsedAsync(int categoryId);
    Task<bool> CategoryExistsAsync(string userGuid, string name);
}