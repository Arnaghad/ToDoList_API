using DataLayer.Contexts;
using DataLayer.Interfaces;
using DataLayer.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataLayer.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DatabaseContext _context;
    private IDbContextTransaction? _transaction;
    
    private ItemRepository? _itemRepository;
    private ICategoryRepository? _categoryRepository;

    public UnitOfWork(DatabaseContext context)
    {
        _context = context;
    }

    public ItemRepository Items
    {
        get
        {
            _itemRepository ??= new ItemRepository(_context);
            return _itemRepository;
        }
    }

    public ICategoryRepository Categories
    {
        get
        {
            _categoryRepository ??= new CategoryRepository(_context);
            return _categoryRepository;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}