using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApi.DTOs;

namespace WebApi.Filters;

/// <summary>
/// Action Filter для автоматичної валідації через FluentValidation
/// </summary>
public class ValidationActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationActionFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Перебираємо всі параметри action методу
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            if (!context.ActionArguments.TryGetValue(parameter.Name, out var argument))
                continue;

            if (argument == null)
                continue;

            // Отримуємо тип валідатора
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            
            // Намагаємось отримати валідатор з DI
            var validator = _serviceProvider.GetService(validatorType) as IValidator;

            if (validator == null)
                continue;

            // Виконуємо валідацію
            var validationContext = new ValidationContext<object>(argument);
            var validationResult = await validator.ValidateAsync(validationContext);

            if (!validationResult.IsValid)
            {
                // Формуємо список помилок
                var errors = validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                // Повертаємо BadRequest з помилками
                context.Result = new BadRequestObjectResult(
                    ApiResponse<object>.ErrorResult("Validation failed", errors));
                return;
            }
        }

        await next();
    }
}