using FluentValidation;
using WebApi.Controllers;
using WebApi.DTOs;
using WebApi.Validators;

namespace WebApi.Extensions;

public static class ValidationExtensions
{
    /// <summary>
    /// Додає FluentValidation з усіма валідаторами
    /// </summary>
    public static IServiceCollection AddFluentValidators(this IServiceCollection services)
    {
        // Додаємо FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateItemDtoValidator>();

        // Реєструємо валідатори явно (опціонально, для кращого контролю)
        services.AddScoped<IValidator<CreateItemDto>, CreateItemDtoValidator>();
        services.AddScoped<IValidator<UpdateItemDto>, UpdateItemDtoValidator>();
        services.AddScoped<IValidator<UpdatePriorityDto>, UpdatePriorityDtoValidator>();
        
        services.AddScoped<IValidator<CreateCategoryDto>, CreateCategoryDtoValidator>();
        services.AddScoped<IValidator<UpdateCategoryDto>, UpdateCategoryDtoValidator>();
        
        services.AddScoped<IValidator<ItemFilterQuery>, ItemFilterQueryValidator>();
        services.AddScoped<IValidator<PaginationQuery>, PaginationQueryValidator>();
        
        services.AddScoped<IValidator<MoveCategoryRequest>, MoveCategoryRequestValidator>();
        services.AddScoped<IValidator<List<int>>, BulkIdListValidator>();
        services.AddScoped<IValidator<Dictionary<int, int>>, BulkUpdatePrioritiesValidator>();

        return services;
    }
}