using AutoMapper;
using BusinessLogic.Interfaces;
using DataLayer.Entities;
using WebApi.DTOs;

namespace WebApi.Mappings;

/// <summary>
/// Профіль AutoMapper для маппінгу Item
/// </summary>
public class ItemMappingProfile : Profile
{
    public ItemMappingProfile()
    {
        // Entity → DTO
        CreateMap<Item, ItemDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryColor, opt => opt.Ignore())
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => 
                src.EndedAt.HasValue && src.EndedAt.Value <= DateTime.UtcNow));

        // CreateDTO → Entity
        CreateMap<CreateItemDto, Item>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserGuid, opt => opt.Ignore());

        // UpdateDTO → Entity (тільки не-null значення)
        CreateMap<UpdateItemDto, Item>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserGuid, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

/// <summary>
/// Профіль AutoMapper для маппінгу Category
/// </summary>
public class CategoryMappingProfile : Profile
{
    public CategoryMappingProfile()
    {
        // Entity → DTO
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ItemsCount, opt => opt.Ignore());

        // CreateDTO → Entity
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserGuid, opt => opt.Ignore());

        // UpdateDTO → Entity (тільки не-null значення)
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserGuid, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Entity → CategoryWithItemsDto
        CreateMap<Category, CategoryWithItemsDto>()
            .ForMember(dest => dest.Items, opt => opt.Ignore())
            .ForMember(dest => dest.TotalItems, opt => opt.Ignore())
            .ForMember(dest => dest.CompletedItems, opt => opt.Ignore())
            .ForMember(dest => dest.PendingItems, opt => opt.Ignore());
    }
}

/// <summary>
///Resolver для додавання інформації про категорію до ItemDto
/// </summary>
public class ItemWithCategoryResolver : IValueResolver<Item, ItemDto, ItemDto>
{
    private readonly ICategoryService _categoryService;

    public ItemWithCategoryResolver(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public ItemDto Resolve(Item source, ItemDto destination, ItemDto destMember, ResolutionContext context)
    {
        if (source.CategoryId.HasValue)
        {
            var category = _categoryService.GetByIdAsync(source.CategoryId.Value).Result;
            if (category != null)
            {
                destination.CategoryName = category.Name;
                destination.CategoryColor = category.Color;
            }
        }

        return destination;
    }
}