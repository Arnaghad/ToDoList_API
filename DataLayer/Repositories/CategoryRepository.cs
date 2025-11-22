using DataLayer.Contexts;
using DataLayer.Entities;
using DataLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetByUserGuidAsync(string userGuid)
    {
        return await _dbSet
            .Where(c => c.UserGuid == userGuid)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByNameAsync(string userGuid, string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.UserGuid == userGuid && c.Name == name);
    }
}