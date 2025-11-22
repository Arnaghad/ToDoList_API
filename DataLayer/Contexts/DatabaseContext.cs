using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DataLayer.Entities;
using DataLayer.Configurations;

namespace DataLayer.Contexts;

public class DatabaseContext : IdentityDbContext<AuthUser>
{
    public DbSet<Item> Items { get; set; }
    public DbSet<Category> Categories { get; set; }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Застосовуємо всі конфігурації з поточної збірки
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
    }
}