using FluentValidation;
using WebApi.DTOs;
using System.Text.RegularExpressions;
using BusinessLogic.Interfaces;

namespace WebApi.Validators;

/// <summary>
/// Валідатор для CreateCategoryDto
/// </summary>
public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    private readonly ICategoryService _categoryService;

    public CreateCategoryDtoValidator(ICategoryService categoryService)
    {
        _categoryService = categoryService;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MinimumLength(1).WithMessage("Category name must be at least 1 character")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters")
            .Must(BeValidCategoryName).WithMessage("Category name contains invalid characters");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Color is required")
            .Must(BeValidHexColor).WithMessage("Color must be a valid hex color (e.g., #FF5733 or #F57)");
    }

    private bool BeValidCategoryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        
        // Дозволяємо літери, цифри, пробіли, дефіси та підкреслення
        return Regex.IsMatch(name, @"^[\w\s\-]+$");
    }

    private bool BeValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;
        
        // Перевіряємо hex колір: #RGB або #RRGGBB
        return Regex.IsMatch(color, @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
    }
}

/// <summary>
/// Валідатор для UpdateCategoryDto
/// </summary>
public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(1).WithMessage("Category name must be at least 1 character")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters")
            .Must(BeValidCategoryName).WithMessage("Category name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Color)
            .Must(BeValidHexColor).WithMessage("Color must be a valid hex color (e.g., #FF5733 or #F57)")
            .When(x => !string.IsNullOrEmpty(x.Color));
    }

    private bool BeValidCategoryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        
        // Дозволяємо літери, цифри, пробіли, дефіси та підкреслення
        return Regex.IsMatch(name, @"^[\w\s\-]+$");
    }

    private bool BeValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;
        
        // Перевіряємо hex колір: #RGB або #RRGGBB
        return Regex.IsMatch(color, @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
    }
}