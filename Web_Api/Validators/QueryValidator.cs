using FluentValidation;
using WebApi.DTOs;

namespace WebApi.Validators;

/// <summary>
/// Валідатор для ItemFilterQuery
/// </summary>
public class ItemFilterQueryValidator : AbstractValidator<ItemFilterQuery>
{
    public ItemFilterQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 10).WithMessage("Priority must be between 1 and 10")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.DueBefore)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(10))
            .WithMessage("Due before date cannot be more than 10 years in the future")
            .When(x => x.DueBefore.HasValue);

        RuleFor(x => x.DueAfter)
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddYears(-10))
            .WithMessage("Due after date cannot be more than 10 years in the past")
            .When(x => x.DueAfter.HasValue);

        RuleFor(x => x)
            .Must(x => !x.DueBefore.HasValue || !x.DueAfter.HasValue || x.DueBefore >= x.DueAfter)
            .WithMessage("Due before date must be after due after date")
            .When(x => x.DueBefore.HasValue && x.DueAfter.HasValue);
    }
}

/// <summary>
/// Валідатор для PaginationQuery
/// </summary>
public class PaginationQueryValidator : AbstractValidator<PaginationQuery>
{
    public PaginationQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
    }
}