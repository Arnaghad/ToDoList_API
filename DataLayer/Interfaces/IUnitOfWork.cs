using DataLayer.Repositories;

namespace DataLayer.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ItemRepository Items { get; }
    ICategoryRepository Categories { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}