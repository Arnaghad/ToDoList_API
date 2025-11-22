using BusinessLogic.Interfaces;
using FluentValidation;
using WebApi.DTOs;
using BusinessLogic.Services;

namespace WebApi.Validators;

/// <summary>
/// Валідатор для CreateItemDto
/// </summary>
public class CreateItemDtoValidator : AbstractValidator<CreateItemDto>
{
    private readonly ICategoryService _categoryService;

    public CreateItemDtoValidator(ICategoryService categoryService)
    {
        _categoryService = categoryService;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Item name is required")
            .MinimumLength(1).WithMessage("Item name must be at least 1 character")
            .MaximumLength(200).WithMessage("Item name cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.AprxHours)
            .GreaterThanOrEqualTo(0).WithMessage("Approximate hours must be 0 or greater")
            .LessThanOrEqualTo(1000).WithMessage("Approximate hours cannot exceed 1000")
            .When(x => x.AprxHours.HasValue);

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10).WithMessage("Priority must be between 1 and 10")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.EndedAt)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("End date cannot be in the past")
            .When(x => x.EndedAt.HasValue);

        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, cancellation) =>
            {
                if (!categoryId.HasValue) return true;
                var category = await _categoryService.GetByIdAsync(categoryId.Value);
                return category != null;
            })
            .WithMessage("Category does not exist")
            .When(x => x.CategoryId.HasValue);
    }
}

/// <summary>
/// Валідатор для UpdateItemDto
/// </summary>
public class UpdateItemDtoValidator : AbstractValidator<UpdateItemDto>
{
    private readonly ICategoryService _categoryService;

    public UpdateItemDtoValidator(ICategoryService categoryService)
    {
        _categoryService = categoryService;

        RuleFor(x => x.Name)
            .MinimumLength(1).WithMessage("Item name must be at least 1 character")
            .MaximumLength(200).WithMessage("Item name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.AprxHours)
            .GreaterThanOrEqualTo(0).WithMessage("Approximate hours must be 0 or greater")
            .LessThanOrEqualTo(1000).WithMessage("Approximate hours cannot exceed 1000")
            .When(x => x.AprxHours.HasValue);

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10).WithMessage("Priority must be between 1 and 10")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.CategoryId)
            .MustAsync(async (categoryId, cancellation) =>
            {
                if (!categoryId.HasValue) return true;
                var category = await _categoryService.GetByIdAsync(categoryId.Value);
                return category != null;
            })
            .WithMessage("Category does not exist")
            .When(x => x.CategoryId.HasValue);
    }
}

/// <summary>
/// Валідатор для UpdatePriorityDto
/// </summary>
public class UpdatePriorityDtoValidator : AbstractValidator<UpdatePriorityDto>
{
    public UpdatePriorityDtoValidator()
    {
        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10)
            .WithMessage("Priority must be between 1 and 10");
    }
}