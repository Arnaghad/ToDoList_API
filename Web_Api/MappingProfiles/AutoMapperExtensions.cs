using AutoMapper;
using DataLayer.Entities;
using WebApi.DTOs;

namespace WebApi.Mappings;

/// <summary>
/// Extension методи для AutoMapper
/// </summary>
public static class AutoMapperExtensions
{
    /// <summary>
    /// Маппінг Item → ItemDto з інформацією про категорію
    /// </summary>
    public static ItemDto ToDto(this Item item, IMapper mapper, Category? category = null)
    {
        var dto = mapper.Map<ItemDto>(item);
        
        if (category != null)
        {
            dto.CategoryName = category.Name;
            dto.CategoryColor = category.Color;
        }
        
        return dto;
    }

    /// <summary>
    /// Маппінг CreateItemDto → Item з UserGuid
    /// </summary>
    public static Item ToEntity(this CreateItemDto dto, IMapper mapper, string userGuid)
    {
        var item = mapper.Map<Item>(dto);
        item.UserGuid = userGuid;
        return item;
    }

    /// <summary>
    /// Оновлення Entity з UpdateItemDto
    /// </summary>
    public static void UpdateEntity(this UpdateItemDto dto, IMapper mapper, Item item)
    {
        mapper.Map(dto, item);
    }

    /// <summary>
    /// Маппінг Category → CategoryDto з кількістю items
    /// </summary>
    public static CategoryDto ToDto(this Category category, IMapper mapper, int itemsCount = 0)
    {
        var dto = mapper.Map<CategoryDto>(category);
        dto.ItemsCount = itemsCount;
        return dto;
    }

    /// <summary>
    /// Маппінг CreateCategoryDto → Category з UserGuid
    /// </summary>
    public static Category ToEntity(this CreateCategoryDto dto, IMapper mapper, string userGuid)
    {
        var category = mapper.Map<Category>(dto);
        category.UserGuid = userGuid;
        return category;
    }

    /// <summary>
    /// Оновлення Entity з UpdateCategoryDto
    /// </summary>
    public static void UpdateEntity(this UpdateCategoryDto dto, IMapper mapper, Category category)
    {
        mapper.Map(dto, category);
    }

    /// <summary>
    /// Маппінг Category → CategoryWithItemsDto
    /// </summary>
    public static CategoryWithItemsDto ToDetailedDto(
        this Category category, 
        IMapper mapper,
        IEnumerable<Item> items)
    {
        var itemsList = items.ToList();
        var completedItems = itemsList.Count(i => i.EndedAt.HasValue && i.EndedAt.Value <= DateTime.UtcNow);

        var dto = mapper.Map<CategoryWithItemsDto>(category);
        dto.Items = itemsList.Select(i => mapper.Map<ItemDto>(i)).ToList();
        dto.TotalItems = itemsList.Count;
        dto.CompletedItems = completedItems;
        dto.PendingItems = itemsList.Count - completedItems;

        // Додаємо інформацію про категорію до кожного item
        foreach (var itemDto in dto.Items)
        {
            itemDto.CategoryName = category.Name;
            itemDto.CategoryColor = category.Color;
        }

        return dto;
    }

    /// <summary>
    /// Маппінг колекції Items → ItemDtos з категоріями
    /// </summary>
    public static async Task<List<ItemDto>> ToDtoListAsync(
        this IEnumerable<Item> items,
        IMapper mapper,
        Func<int, Task<Category?>> getCategoryFunc)
    {
        var result = new List<ItemDto>();

        foreach (var item in items)
        {
            var dto = mapper.Map<ItemDto>(item);
            
            if (item.CategoryId.HasValue)
            {
                var category = await getCategoryFunc(item.CategoryId.Value);
                if (category != null)
                {
                    dto.CategoryName = category.Name;
                    dto.CategoryColor = category.Color;
                }
            }
            
            result.Add(dto);
        }

        return result;
    }
}