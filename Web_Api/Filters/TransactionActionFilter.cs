using DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace WebApi.Filters;

/// <summary>
/// Action Filter для автоматичного управління транзакціями на рівні контролера
/// </summary>
public class TransactionActionFilter : IAsyncActionFilter
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionActionFilter> _logger;

    public TransactionActionFilter(IUnitOfWork unitOfWork, ILogger<TransactionActionFilter> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Перевіряємо наявність атрибута UseTransaction
        var useTransaction = context.ActionDescriptor.EndpointMetadata
            .OfType<UseTransactionAttribute>()
            .FirstOrDefault();

        if (useTransaction == null)
        {
            await next();
            return;
        }

        // GET запити не потребують транзакцій
        if (context.HttpContext.Request.Method == HttpMethod.Get.Method)
        {
            await next();
            return;
        }

        var actionName = $"{context.Controller.GetType().Name}.{context.ActionDescriptor.DisplayName}";
        _logger.LogInformation("Starting transaction for action: {ActionName}", actionName);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var executedContext = await next();

            if (executedContext.Exception == null)
            {
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Transaction committed for action: {ActionName}", actionName);
            }
            else
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(executedContext.Exception, 
                    "Transaction rolled back due to exception in action: {ActionName}", actionName);
            }
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Transaction rolled back for action: {ActionName}", actionName);
            throw;
        }
    }
}

/// <summary>
/// Атрибут для позначення що метод використовує транзакції через Filter
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
public class UseTransactionAttribute : Attribute, IFilterMetadata
{
}