using DataLayer.Contexts;
using DataLayer.Entities;
using DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories;

public class ItemRepository : Repository<Item>, IItemRepository
{
    public ItemRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Item>> GetByUserGuidAsync(string userGuid)
    {
        return await _dbSet
            .Where(i => i.UserGuid == userGuid)
            .OrderBy(i => i.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetByCategoryIdAsync(int categoryId)
    {
        return await _dbSet
            .Where(i => i.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetLoopedItemsAsync(string userGuid)
    {
        return await _dbSet
            .Where(i => i.UserGuid == userGuid && i.IsLooped == true)
            .ToListAsync();
    }

    public async Task<IEnumerable<Item>> GetByPriorityAsync(string userGuid, int priority)
    {
        return await _dbSet
            .Where(i => i.UserGuid == userGuid && i.Priority == priority)
            .ToListAsync();
    }
}