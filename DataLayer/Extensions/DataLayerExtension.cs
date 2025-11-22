using DataLayer.Contexts;
using DataLayer.Interfaces;
using DataLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataLayer.Extensions;

public static class DataLayerExtensions
{
    public static IServiceCollection AddContext(
        this IServiceCollection services,
        string connectionString,
        string migrationsAssembly = "")
    {
        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                if (!string.IsNullOrEmpty(migrationsAssembly))
                {
                    npgsqlOptions.MigrationsAssembly(migrationsAssembly);
                }
            });
        });

        return services;
    }

    public static IServiceCollection AddContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string migrationsAssembly = "")
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        return services.AddContext(connectionString!, migrationsAssembly);
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        return services;
    }

    public static IServiceCollection AddDataLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string migrationsAssembly = "")
    {
        services.AddContext(configuration, migrationsAssembly);
        services.AddRepositories();

        return services;
    }
}