namespace WebApi.DTOs;

// DTO для отримання Item
public class ItemDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? AprxHours { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? Priority { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryColor { get; set; }
    public bool? IsLooped { get; set; }
    public string UserGuid { get; set; } = string.Empty;
    public bool IsCompleted => EndedAt.HasValue && EndedAt.Value <= DateTime.UtcNow;
}

// DTO для створення Item
public class CreateItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AprxHours { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? Priority { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsLooped { get; set; }
}

// DTO для оновлення Item
public class UpdateItemDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? AprxHours { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? Priority { get; set; }
    public int? CategoryId { get; set; }
    public bool? IsLooped { get; set; }
}

// DTO для зміни пріоритету
public class UpdatePriorityDto
{
    public int Priority { get; set; }
}

// DTO для статистики Item
public class ItemStatisticsDto
{
    public int TotalItems { get; set; }
    public int CompletedItems { get; set; }
    public int PendingItems { get; set; }
    public int LoopedItems { get; set; }
    public double CompletionRate { get; set; }
    public int TotalEstimatedHours { get; set; }
}