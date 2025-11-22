using BusinessLogic.Interfaces;
using DataLayer.Entities;
using DataLayer.Interfaces;

namespace BusinessLogic.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Categories.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _unitOfWork.Categories.GetAllAsync();
    }

    public async Task<IEnumerable<Category>> GetByUserGuidAsync(string userGuid)
    {
        return await _unitOfWork.Categories.GetByUserGuidAsync(userGuid);
    }

    public async Task<Category?> GetByNameAsync(string userGuid, string name)
    {
        return await _unitOfWork.Categories.GetByNameAsync(userGuid, name);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            throw new ArgumentException("Category name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(category.Color))
        {
            throw new ArgumentException("Category color cannot be empty");
        }

        var existingCategory = await _unitOfWork.Categories
            .GetByNameAsync(category.UserGuid, category.Name);

        if (existingCategory != null)
        {
            throw new InvalidOperationException(
                $"Category with name '{category.Name}' already exists for this user");
        }

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return category;
    }

    public async Task<Category?> UpdateAsync(int id, Category category)
    {
        var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id);
        if (existingCategory == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(category.Name))
        {
            var duplicateCategory = await _unitOfWork.Categories
                .GetByNameAsync(existingCategory.UserGuid, category.Name);

            if (duplicateCategory != null && duplicateCategory.Id != id)
            {
                throw new InvalidOperationException(
                    $"Category with name '{category.Name}' already exists for this user");
            }

            existingCategory.Name = category.Name;
        }

        if (!string.IsNullOrWhiteSpace(category.Color))
        {
            existingCategory.Color = category.Color;
        }

        _unitOfWork.Categories.Update(existingCategory);
        await _unitOfWork.SaveChangesAsync();

        return existingCategory;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            return false;
        }

        var hasItems = await IsCategoryUsedAsync(id);
        if (hasItems)
        {
            throw new InvalidOperationException(
                "Cannot delete category that has associated items. Delete items first or use DeleteWithItemsAsync.");
        }

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteWithItemsAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
        {
            return false;
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var items = await _unitOfWork.Items.GetByCategoryIdAsync(id);
            
            foreach (var item in items)
            {
                item.CategoryId = null;
            }

            _unitOfWork.Items.UpdateRange(items);
            _unitOfWork.Categories.Remove(category);

            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<int> GetItemsCountAsync(int categoryId)
    {
        var items = await _unitOfWork.Items.GetByCategoryIdAsync(categoryId);
        return items.Count();
    }

    public async Task<bool> IsCategoryUsedAsync(int categoryId)
    {
        return await _unitOfWork.Items.AnyAsync(i => i.CategoryId == categoryId);
    }

    public async Task<bool> CategoryExistsAsync(string userGuid, string name)
    {
        var category = await _unitOfWork.Categories.GetByNameAsync(userGuid, name);
        return category != null;
    }
}