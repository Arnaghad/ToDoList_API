using DataLayer.Entities;
using DataLayer.Interfaces;

namespace BusinessLogic.Interfaces;

public interface IItemService
{
    Task<Item?> GetByIdAsync(int id);
    Task<IEnumerable<Item>> GetAllAsync();
    Task<IEnumerable<Item>> GetByUserGuidAsync(string userGuid);
    Task<IEnumerable<Item>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Item>> GetLoopedItemsAsync(string userGuid);
    Task<IEnumerable<Item>> GetByPriorityAsync(string userGuid, int priority);
    Task<Item> CreateAsync(Item item);
    Task<Item?> UpdateAsync(int id, Item item);
    Task<bool> DeleteAsync(int id);
    Task<bool> ToggleLoopAsync(int id);
    Task<bool> UpdatePriorityAsync(int id, int newPriority);
    Task<IEnumerable<Item>> GetPendingItemsAsync(string userGuid);
    Task<IEnumerable<Item>> GetCompletedItemsAsync(string userGuid);
    Task<bool> CompleteItemAsync(int id);
}