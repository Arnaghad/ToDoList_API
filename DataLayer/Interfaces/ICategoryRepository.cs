using DataLayer.Entities;

namespace DataLayer.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetByUserGuidAsync(string userGuid);
    Task<Category?> GetByNameAsync(string userGuid, string name);
}