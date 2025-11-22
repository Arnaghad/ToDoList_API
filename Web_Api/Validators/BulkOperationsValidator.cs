using FluentValidation;
using WebApi.Controllers;
using WebApi.DTOs;

namespace WebApi.Validators;

/// <summary>
/// Валідатор для MoveCategoryRequest
/// </summary>
public class MoveCategoryRequestValidator : AbstractValidator<MoveCategoryRequest>
{
    public MoveCategoryRequestValidator()
    {
        RuleFor(x => x.FromCategoryId)
            .GreaterThan(0).WithMessage("From category ID must be greater than 0");

        RuleFor(x => x.ToCategoryId)
            .GreaterThan(0).WithMessage("To category ID must be greater than 0");

        RuleFor(x => x)
            .Must(x => x.FromCategoryId != x.ToCategoryId)
            .WithMessage("From and To category IDs must be different");
    }
}

/// <summary>
/// Валідатор для списку ID (bulk delete, bulk complete, etc.)
/// </summary>
public class BulkIdListValidator : AbstractValidator<List<int>>
{
    public BulkIdListValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("ID list cannot be empty")
            .Must(x => x.Count <= 100).WithMessage("Cannot process more than 100 items at once")
            .Must(x => x.All(id => id > 0)).WithMessage("All IDs must be greater than 0")
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("Duplicate IDs are not allowed");
    }
}

/// <summary>
/// Валідатор для списку CreateItemDto (bulk create)
/// </summary>
public class BulkCreateItemListValidator : AbstractValidator<List<CreateItemDto>>
{
    public BulkCreateItemListValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("Item list cannot be empty")
            .Must(x => x.Count <= 50).WithMessage("Cannot create more than 50 items at once");

        RuleForEach(x => x)
            .SetValidator(new CreateItemDtoValidator(null!)); // Validator буде resolved через DI
    }
}

/// <summary>
/// Валідатор для Dictionary<int, int> (bulk update priorities)
/// </summary>
public class BulkUpdatePrioritiesValidator : AbstractValidator<Dictionary<int, int>>
{
    public BulkUpdatePrioritiesValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("Priorities dictionary cannot be empty")
            .Must(x => x.Count <= 100).WithMessage("Cannot update more than 100 items at once")
            .Must(x => x.Keys.All(id => id > 0)).WithMessage("All item IDs must be greater than 0")
            .Must(x => x.Values.All(p => p >= 1 && p <= 10)).WithMessage("All priorities must be between 1 and 10");
    }
}