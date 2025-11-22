using BusinessLogic.Interfaces;
using DataLayer.Entities;
using DataLayer.Interfaces;

namespace BusinessLogic.Services;

public class ItemService : IItemService
{
    private readonly IUnitOfWork _unitOfWork;

    public ItemService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Items.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await _unitOfWork.Items.GetAllAsync();
    }

    public async Task<IEnumerable<Item>> GetByUserGuidAsync(string userGuid)
    {
        return await _unitOfWork.Items.GetByUserGuidAsync(userGuid);
    }

    public async Task<IEnumerable<Item>> GetByCategoryIdAsync(int categoryId)
    {
        return await _unitOfWork.Items.GetByCategoryIdAsync(categoryId);
    }

    public async Task<IEnumerable<Item>> GetLoopedItemsAsync(string userGuid)
    {
        return await _unitOfWork.Items.GetLoopedItemsAsync(userGuid);
    }

    public async Task<IEnumerable<Item>> GetByPriorityAsync(string userGuid, int priority)
    {
        return await _unitOfWork.Items.GetByPriorityAsync(userGuid, priority);
    }

    public async Task<Item> CreateAsync(Item item)
    {
        if (string.IsNullOrWhiteSpace(item.Name))
        {
            throw new ArgumentException("Item name cannot be empty");
        }

        if (item.CategoryId.HasValue)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(item.CategoryId.Value);
            if (category == null)
            {
                throw new ArgumentException($"Category with ID {item.CategoryId} does not exist");
            }
        }

        await _unitOfWork.Items.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return item;
    }

    public async Task<Item?> UpdateAsync(int id, Item item)
    {
        var existingItem = await _unitOfWork.Items.GetByIdAsync(id);
        if (existingItem == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(item.Name))
        {
            existingItem.Name = item.Name;
        }

        existingItem.Description = item.Description;
        existingItem.AprxHours = item.AprxHours;
        existingItem.EndedAt = item.EndedAt;
        existingItem.Priority = item.Priority;
        existingItem.CategoryId = item.CategoryId;
        existingItem.IsLooped = item.IsLooped;

        _unitOfWork.Items.Update(existingItem);
        await _unitOfWork.SaveChangesAsync();

        return existingItem;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id);
        if (item == null)
        {
            return false;
        }

        _unitOfWork.Items.Remove(item);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleLoopAsync(int id)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id);
        if (item == null)
        {
            return false;
        }

        item.IsLooped = !(item.IsLooped ?? false);
        _unitOfWork.Items.Update(item);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdatePriorityAsync(int id, int newPriority)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id);
        if (item == null)
        {
            return false;
        }

        item.Priority = newPriority;
        _unitOfWork.Items.Update(item);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<Item>> GetPendingItemsAsync(string userGuid)
    {
        var items = await _unitOfWork.Items.GetByUserGuidAsync(userGuid);
        return items.Where(i => !i.EndedAt.HasValue || i.EndedAt.Value > DateTime.UtcNow);
    }

    public async Task<IEnumerable<Item>> GetCompletedItemsAsync(string userGuid)
    {
        var items = await _unitOfWork.Items.GetByUserGuidAsync(userGuid);
        return items.Where(i => i.EndedAt.HasValue && i.EndedAt.Value <= DateTime.UtcNow);
    }

    public async Task<bool> CompleteItemAsync(int id)
    {
        var item = await _unitOfWork.Items.GetByIdAsync(id);
        if (item == null)
        {
            return false;
        }

        item.EndedAt = DateTime.UtcNow;
        _unitOfWork.Items.Update(item);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}