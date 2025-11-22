using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BusinessLogic.Interfaces;
using WebApi.DTOs;
using WebApi.Filters;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BulkOperationsController : ControllerBase
{
    private readonly IBulkOperationsService _bulkService;
    private readonly IItemService _itemService;

    public BulkOperationsController(
        IBulkOperationsService bulkService,
        IItemService itemService)
    {
        _bulkService = bulkService;
        _itemService = itemService;
    }

    private string GetUserGuid()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new UnauthorizedAccessException("User not authenticated");
    }

    /// <summary>
    /// Масове створення items
    /// </summary>
    [HttpPost("items/bulk-create")]
    [UseTransaction]
    public async Task<ActionResult<ApiResponse<BulkOperationResult>>> BulkCreateItems(
        [FromBody] List<CreateItemDto> dtos)
    {
        try
        {
            var userGuid = GetUserGuid();
            var items = dtos.Select(dto => new DataLayer.Entities.Item
            {
                Name = dto.Name,
                Description = dto.Description,
                AprxHours = dto.AprxHours,
                EndedAt = dto.EndedAt,
                Priority = dto.Priority,
                CategoryId = dto.CategoryId,
                IsLooped = dto.IsLooped,
                UserGuid = userGuid
            });

            var result = await _bulkService.BulkCreateItemsAsync(items);

            return result.Success 
                ? Ok(ApiResponse<BulkOperationResult>.SuccessResult(result))
                : BadRequest(ApiResponse<BulkOperationResult>.ErrorResult(result.Message, result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BulkOperationResult>.ErrorResult(
                "Failed to bulk create items", ex.Message));
        }
    }

    /// <summary>
    /// Масове видалення items
    /// </summary>
    [HttpPost("items/bulk-delete")]
    [UseTransaction]
    public async Task<ActionResult<ApiResponse<BulkOperationResult>>> BulkDeleteItems(
        [FromBody] List<int> itemIds)
    {
        try
        {
            var userGuid = GetUserGuid();

            // Перевіряємо що всі items належать користувачу
            foreach (var id in itemIds)
            {
                var item = await _itemService.GetByIdAsync(id);
                if (item == null || item.UserGuid != userGuid)
                {
                    return Forbid();
                }
            }

            var result = await _bulkService.BulkDeleteItemsAsync(itemIds);

            return result.Success 
                ? Ok(ApiResponse<BulkOperationResult>.SuccessResult(result))
                : BadRequest(ApiResponse<BulkOperationResult>.ErrorResult(result.Message, result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BulkOperationResult>.ErrorResult(
                "Failed to bulk delete items", ex.Message));
        }
    }

    /// <summary>
    /// Масове завершення items
    /// </summary>
    [HttpPost("items/bulk-complete")]
    [UseTransaction]
    public async Task<ActionResult<ApiResponse<BulkOperationResult>>> BulkCompleteItems(
        [FromBody] List<int> itemIds)
    {
        try
        {
            var userGuid = GetUserGuid();

            // Перевіряємо що всі items належать користувачу
            foreach (var id in itemIds)
            {
                var item = await _itemService.GetByIdAsync(id);
                if (item == null || item.UserGuid != userGuid)
                {
                    return Forbid();
                }
            }

            var result = await _bulkService.CompleteMultipleItemsAsync(itemIds);

            return result.Success 
                ? Ok(ApiResponse<BulkOperationResult>.SuccessResult(result))
                : BadRequest(ApiResponse<BulkOperationResult>.ErrorResult(result.Message, result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BulkOperationResult>.ErrorResult(
                "Failed to bulk complete items", ex.Message));
        }
    }

    /// <summary>
    /// Переміщення items між категоріями
    /// </summary>
    [HttpPost("items/move-category")]
    [UseTransaction]
    public async Task<ActionResult<ApiResponse<BulkOperationResult>>> MoveCategoryItems(
        [FromBody] MoveCategoryRequest request)
    {
        try
        {
            var result = await _bulkService.MoveCategoryItemsAsync(
                request.FromCategoryId, 
                request.ToCategoryId);

            return result.Success 
                ? Ok(ApiResponse<BulkOperationResult>.SuccessResult(result))
                : BadRequest(ApiResponse<BulkOperationResult>.ErrorResult(result.Message, result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BulkOperationResult>.ErrorResult(
                "Failed to move items", ex.Message));
        }
    }

    /// <summary>
    /// Масове оновлення пріоритетів
    /// </summary>
    [HttpPost("items/bulk-update-priorities")]
    [UseTransaction]
    public async Task<ActionResult<ApiResponse<BulkOperationResult>>> BulkUpdatePriorities(
        [FromBody] Dictionary<int, int> itemPriorities)
    {
        try
        {
            var userGuid = GetUserGuid();

            // Перевіряємо що всі items належать користувачу
            foreach (var itemId in itemPriorities.Keys)
            {
                var item = await _itemService.GetByIdAsync(itemId);
                if (item == null || item.UserGuid != userGuid)
                {
                    return Forbid();
                }
            }

            var result = await _bulkService.UpdateMultiplePrioritiesAsync(itemPriorities);

            return result.Success 
                ? Ok(ApiResponse<BulkOperationResult>.SuccessResult(result))
                : BadRequest(ApiResponse<BulkOperationResult>.ErrorResult(result.Message, result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BulkOperationResult>.ErrorResult(
                "Failed to update priorities", ex.Message));
        }
    }

    /// <summary>
    /// Дублювання items
    /// </summary>
    [HttpPost("items/duplicate")]
    [UseTransaction]
    public async Task<ActionResult<ApiResponse<BulkOperationResult>>> DuplicateItems(
        [FromBody] List<int> itemIds)
    {
        try
        {
            var userGuid = GetUserGuid();

            // Перевіряємо що всі items належать користувачу
            foreach (var id in itemIds)
            {
                var item = await _itemService.GetByIdAsync(id);
                if (item == null || item.UserGuid != userGuid)
                {
                    return Forbid();
                }
            }

            var result = await _bulkService.DuplicateItemsAsync(itemIds, userGuid);

            return result.Success 
                ? Ok(ApiResponse<BulkOperationResult>.SuccessResult(result))
                : BadRequest(ApiResponse<BulkOperationResult>.ErrorResult(result.Message, result.Errors));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<BulkOperationResult>.ErrorResult(
                "Failed to duplicate items", ex.Message));
        }
    }
}

public class MoveCategoryRequest
{
    public int FromCategoryId { get; set; }
    public int ToCategoryId { get; set; }
}