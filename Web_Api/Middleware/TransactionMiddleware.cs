using DataLayer.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebApi.Middleware;

/// <summary>
/// Middleware для автоматичного управління транзакціями
/// Використовується для Minimal API endpoints
/// </summary>
public class TransactionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TransactionMiddleware> _logger;

    public TransactionMiddleware(RequestDelegate next, ILogger<TransactionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        // Якщо це GET запит - не використовуємо транзакції
        if (context.Request.Method == HttpMethod.Get.Method)
        {
            await _next(context);
            return;
        }

        _logger.LogInformation("Starting transaction for {Method} {Path}", 
            context.Request.Method, context.Request.Path);

        try
        {
            // Починаємо транзакцію
            await unitOfWork.BeginTransactionAsync();

            // Виконуємо запит
            await _next(context);

            // Якщо статус код успішний (2xx) - комітимо
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                await unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Transaction committed successfully for {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
            }
            else
            {
                await unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("Transaction rolled back due to status code {StatusCode} for {Method} {Path}", 
                    context.Response.StatusCode, context.Request.Method, context.Request.Path);
            }
        }
        catch (Exception ex)
        {
            // При помилці - відкочуємо транзакцію
            await unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Transaction rolled back due to exception for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            throw;
        }
    }
}