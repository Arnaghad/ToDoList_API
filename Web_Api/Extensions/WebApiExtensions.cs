using WebApi.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Extensions;

public static class WebApiExtensions
{
    /// <summary>
    /// Додає AutoMapper з усіма профілями
    /// </summary>
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
    {
        services.AddAutoMapper(config =>
        {
            config.AddProfile<ItemMappingProfile>();
            config.AddProfile<CategoryMappingProfile>();
        }, typeof(Program).Assembly);

        return services;
    }
}