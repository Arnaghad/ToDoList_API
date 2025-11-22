using DataLayer.Entities;

namespace DataLayer.Interfaces;

public interface IItemRepository : IRepository<Item>
{
    Task<IEnumerable<Item>> GetByUserGuidAsync(string userGuid);
    Task<IEnumerable<Item>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Item>> GetLoopedItemsAsync(string userGuid);
    Task<IEnumerable<Item>> GetByPriorityAsync(string userGuid, int priority);
}
