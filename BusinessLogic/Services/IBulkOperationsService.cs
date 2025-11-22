using DataLayer.Entities;
using DataLayer.Interfaces;

namespace BusinessLogic.Services;

public interface IBulkOperationsService
{
    Task<BulkOperationResult> BulkCreateItemsAsync(IEnumerable<Item> items);
    Task<BulkOperationResult> BulkUpdateItemsAsync(IEnumerable<Item> items);
    Task<BulkOperationResult> BulkDeleteItemsAsync(IEnumerable<int> itemIds);
    Task<BulkOperationResult> MoveCategoryItemsAsync(int fromCategoryId, int toCategoryId);
    Task<BulkOperationResult> CompleteMultipleItemsAsync(IEnumerable<int> itemIds);
    Task<BulkOperationResult> UpdateMultiplePrioritiesAsync(Dictionary<int, int> itemPriorities);
    Task<BulkOperationResult> DuplicateItemsAsync(IEnumerable<int> itemIds, string userGuid);
}

public class BulkOperationsService : IBulkOperationsService
{
    private readonly IUnitOfWork _unitOfWork;

    public BulkOperationsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkOperationResult> BulkCreateItemsAsync(IEnumerable<Item> items)
    {
        var result = new BulkOperationResult();

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var itemsList = items.ToList();
            await _unitOfWork.Items.AddRangeAsync(itemsList);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            result.Success = true;
            result.AffectedCount = itemsList.Count;
            result.Message = $"Successfully created {itemsList.Count} items";
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            result.Success = false;
            result.Message = $"Failed to create items: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<BulkOperationResult> BulkUpdateItemsAsync(IEnumerable<Item> items)
    {
        var result = new BulkOperationResult();

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var itemsList = items.ToList();
            _unitOfWork.Items.UpdateRange(itemsList);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            result.Success = true;
            result.AffectedCount = itemsList.Count;
            result.Message = $"Successfully updated {itemsList.Count} items";
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            result.Success = false;
            result.Message = $"Failed to update items: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<BulkOperationResult> BulkDeleteItemsAsync(IEnumerable<int> itemIds)
    {
        var result = new BulkOperationResult();

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var items = new List<Item>();
            foreach (var id in itemIds)
            {
                var item = await _unitOfWork.Items.GetByIdAsync(id);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            if (items.Any())
            {
                _unitOfWork.Items.RemoveRange(items);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitTransactionAsync();

            result.Success = true;
            result.AffectedCount = items.Count;
            result.Message = $"Successfully deleted {items.Count} items";
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            result.Success = false;
            result.Message = $"Failed to delete items: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<BulkOperationResult> MoveCategoryItemsAsync(int fromCategoryId, int toCategoryId)
    {
        var result = new BulkOperationResult();

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Перевіряємо чи існує цільова категорія
            var targetCategory = await _unitOfWork.Categories.GetByIdAsync(toCategoryId);
            if (targetCategory == null)
            {
                throw new ArgumentException($"Target category with ID {toCategoryId} does not exist");
            }

            // Отримуємо всі items з вихідної категорії
            var items = await _unitOfWork.Items.GetByCategoryIdAsync(fromCategoryId);
            var itemsList = items.ToList();

            // Переміщуємо items
            foreach (var item in itemsList)
            {
                item.CategoryId = toCategoryId;
            }

            _unitOfWork.Items.UpdateRange(itemsList);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            result.Success = true;
            result.AffectedCount = itemsList.Count;
            result.Message = $"Successfully moved {itemsList.Count} items from category {fromCategoryId} to {toCategoryId}";
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            result.Success = false;
            result.Message = $"Failed to move items: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<BulkOperationResult> CompleteMultipleItemsAsync(IEnumerable<int> itemIds)
    {
        var result = new BulkOperationResult();

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var items = new List<Item>();
            var now = DateTime.UtcNow;

            foreach (var id in itemIds)
            {
                var item = await _unitOfWork.Items.GetByIdAsync(id);
                if (item != null)
                {
                    item.EndedAt = now;
                    items.Add(item);
                }
            }

            if (items.Any())
            {
                _unitOfWork.Items.UpdateRange(items);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitTransactionAsync();

            result.Success = true;
            result.AffectedCount = items.Count;
            result.Message = $"Successfully completed {items.Count} items";
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            result.Success = false;
            result.Message = $"Failed to complete items: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<BulkOperationResult> UpdateMultiplePrioritiesAsync(Dictionary<int, int> itemPriorities)
    {
        var result = new BulkOperationResult();

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var items = new List<Item>();

            foreach (var (itemId, priority) in itemPriorities)
            {
                var item = await _unitOfWork.Items.GetByIdAsync(itemId);
                if (item != null)
                {
                    item.Priority = priority;
                    items.Add(item);
                }
            }

            if (items.Any())
            {
                _unitOfWork.Items.UpdateRange(items);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitTransactionAsync();

            result.Success = true;
            result.AffectedCount = items.Count;
            result.Message = $"Successfully updated priorities for {items.Count} items";
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            result.Success = false;
            result.Message = $"Failed to update priorities: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    public async Task<BulkOperationResult> DuplicateItemsAsync(IEnumerable<int> itemIds, string userGuid)
    {
        var result = new BulkOperationResult();

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var newItems = new List<Item>();

            foreach (var id in itemIds)
            {
                var originalItem = await _unitOfWork.Items.GetByIdAsync(id);
                if (originalItem != null && originalItem.UserGuid == userGuid)
                {
                    var duplicatedItem = new Item
                    {
                        Name = $"{originalItem.Name} (Copy)",
                        Description = originalItem.Description,
                        AprxHours = originalItem.AprxHours,
                        Priority = originalItem.Priority,
                        CategoryId = originalItem.CategoryId,
                        IsLooped = originalItem.IsLooped,
                        UserGuid = userGuid,
                        EndedAt = null // Скидаємо дату завершення для копії
                    };

                    newItems.Add(duplicatedItem);
                }
            }

            if (newItems.Any())
            {
                await _unitOfWork.Items.AddRangeAsync(newItems);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitTransactionAsync();

            result.Success = true;
            result.AffectedCount = newItems.Count;
            result.Message = $"Successfully duplicated {newItems.Count} items";
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            result.Success = false;
            result.Message = $"Failed to duplicate items: {ex.Message}";
            result.Errors.Add(ex.Message);
        }

        return result;
    }
}

public class BulkOperationResult
{
    public bool Success { get; set; }
    public int AffectedCount { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}