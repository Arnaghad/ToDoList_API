namespace WebApi.DTOs;

// DTO для отримання Category
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string UserGuid { get; set; } = string.Empty;
    public int ItemsCount { get; set; }
}

// DTO для створення Category
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

// DTO для оновлення Category
public class UpdateCategoryDto
{
    public string? Name { get; set; }
    public string? Color { get; set; }
}

// DTO для Category з повною інформацією про Items
public class CategoryWithItemsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string UserGuid { get; set; } = string.Empty;
    public List<ItemDto> Items { get; set; } = new();
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public int PendingItems { get; set; }
}