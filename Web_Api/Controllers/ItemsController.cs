using BusinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;
using WebApi.Mappings;
using AutoMapper;
using BusinessLogic.Interfaces;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;

    public ItemsController(
        IItemService itemService, 
        ICategoryService categoryService,
        IMapper mapper)
    {
        _itemService = itemService;
        _categoryService = categoryService;
        _mapper = mapper;
    }

    private string GetUserGuid()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new UnauthorizedAccessException("User not authenticated");
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ItemDto>>>> GetAll([FromQuery] ItemFilterQuery query)
    {
        try
        {
            var userGuid = GetUserGuid();
            var items = await _itemService.GetByUserGuidAsync(userGuid);

            // Фільтрація
            if (query.CategoryId.HasValue)
                items = items.Where(i => i.CategoryId == query.CategoryId.Value);

            if (query.Priority.HasValue)
                items = items.Where(i => i.Priority == query.Priority.Value);

            if (query.IsLooped.HasValue)
                items = items.Where(i => i.IsLooped == query.IsLooped.Value);

            if (query.IsCompleted.HasValue)
            {
                var now = DateTime.UtcNow;
                items = query.IsCompleted.Value
                    ? items.Where(i => i.EndedAt.HasValue && i.EndedAt.Value <= now)
                    : items.Where(i => !i.EndedAt.HasValue || i.EndedAt.Value > now);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                items = items.Where(i => i.Name != null && i.Name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase));

            if (query.DueBefore.HasValue)
                items = items.Where(i => i.EndedAt.HasValue && i.EndedAt.Value <= query.DueBefore.Value);

            if (query.DueAfter.HasValue)
                items = items.Where(i => i.EndedAt.HasValue && i.EndedAt.Value >= query.DueAfter.Value);

            // Маппінг з категоріями
            var itemDtos = await items.ToDtoListAsync(_mapper, 
                id => _categoryService.GetByIdAsync(id));

            return Ok(ApiResponse<List<ItemDto>>.SuccessResult(itemDtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ItemDto>>.ErrorResult("Failed to retrieve items", ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ItemDto>>> GetById(int id)
    {
        try
        {
            var item = await _itemService.GetByIdAsync(id);
            
            if (item == null)
                return NotFound(ApiResponse<ItemDto>.ErrorResult("Item not found"));

            if (item.UserGuid != GetUserGuid())
                return Forbid();

            var category = item.CategoryId.HasValue 
                ? await _categoryService.GetByIdAsync(item.CategoryId.Value) 
                : null;

            var itemDto = item.ToDto(_mapper, category);

            return Ok(ApiResponse<ItemDto>.SuccessResult(itemDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ItemDto>.ErrorResult("Failed to retrieve item", ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ItemDto>>> Create([FromBody] CreateItemDto dto)
    {
        try
        {
            var userGuid = GetUserGuid();
            var item = dto.ToEntity(_mapper, userGuid);

            var created = await _itemService.CreateAsync(item);
            var category = created.CategoryId.HasValue 
                ? await _categoryService.GetByIdAsync(created.CategoryId.Value) 
                : null;

            var createdDto = created.ToDto(_mapper, category);

            return CreatedAtAction(
                nameof(GetById), 
                new { id = created.Id }, 
                ApiResponse<ItemDto>.SuccessResult(createdDto, "Item created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ItemDto>.ErrorResult("Validation failed", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ItemDto>.ErrorResult("Failed to create item", ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ItemDto>>> Update(int id, [FromBody] UpdateItemDto dto)
    {
        try
        {
            var item = await _itemService.GetByIdAsync(id);
            
            if (item == null)
                return NotFound(ApiResponse<ItemDto>.ErrorResult("Item not found"));

            if (item.UserGuid != GetUserGuid())
                return Forbid();

            // Правильний порядок: dto.UpdateEntity(_mapper, item)
            dto.UpdateEntity(_mapper, item);
            var updated = await _itemService.UpdateAsync(id, item);

            if (updated == null)
                return NotFound(ApiResponse<ItemDto>.ErrorResult("Item not found"));

            var category = updated.CategoryId.HasValue 
                ? await _categoryService.GetByIdAsync(updated.CategoryId.Value) 
                : null;

            var updatedDto = updated.ToDto(_mapper, category);

            return Ok(ApiResponse<ItemDto>.SuccessResult(updatedDto, "Item updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ItemDto>.ErrorResult("Failed to update item", ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var item = await _itemService.GetByIdAsync(id);
            
            if (item == null)
                return NotFound(ApiResponse<bool>.ErrorResult("Item not found"));

            if (item.UserGuid != GetUserGuid())
                return Forbid();

            var result = await _itemService.DeleteAsync(id);
            
            return result
                ? Ok(ApiResponse<bool>.SuccessResult(true, "Item deleted successfully"))
                : NotFound(ApiResponse<bool>.ErrorResult("Item not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Failed to delete item", ex.Message));
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<ApiResponse<bool>>> Complete(int id)
    {
        try
        {
            var item = await _itemService.GetByIdAsync(id);
            
            if (item == null)
                return NotFound(ApiResponse<bool>.ErrorResult("Item not found"));

            if (item.UserGuid != GetUserGuid())
                return Forbid();

            var result = await _itemService.CompleteItemAsync(id);
            
            return result
                ? Ok(ApiResponse<bool>.SuccessResult(true, "Item completed successfully"))
                : NotFound(ApiResponse<bool>.ErrorResult("Item not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Failed to complete item", ex.Message));
        }
    }

    [HttpPost("{id}/toggle-loop")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleLoop(int id)
    {
        try
        {
            var item = await _itemService.GetByIdAsync(id);
            
            if (item == null)
                return NotFound(ApiResponse<bool>.ErrorResult("Item not found"));

            if (item.UserGuid != GetUserGuid())
                return Forbid();

            var result = await _itemService.ToggleLoopAsync(id);
            
            return result
                ? Ok(ApiResponse<bool>.SuccessResult(true, "Loop status toggled successfully"))
                : NotFound(ApiResponse<bool>.ErrorResult("Item not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Failed to toggle loop", ex.Message));
        }
    }

    [HttpPatch("{id}/priority")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdatePriority(int id, [FromBody] UpdatePriorityDto dto)
    {
        try
        {
            var item = await _itemService.GetByIdAsync(id);
            
            if (item == null)
                return NotFound(ApiResponse<bool>.ErrorResult("Item not found"));

            if (item.UserGuid != GetUserGuid())
                return Forbid();

            var result = await _itemService.UpdatePriorityAsync(id, dto.Priority);
            
            return result
                ? Ok(ApiResponse<bool>.SuccessResult(true, "Priority updated successfully"))
                : NotFound(ApiResponse<bool>.ErrorResult("Item not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Failed to update priority", ex.Message));
        }
    }

    [HttpGet("looped")]
    public async Task<ActionResult<ApiResponse<List<ItemDto>>>> GetLooped()
    {
        try
        {
            var userGuid = GetUserGuid();
            var items = await _itemService.GetLoopedItemsAsync(userGuid);
            
            var itemDtos = await items.ToDtoListAsync(_mapper, 
                id => _categoryService.GetByIdAsync(id));

            return Ok(ApiResponse<List<ItemDto>>.SuccessResult(itemDtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ItemDto>>.ErrorResult("Failed to retrieve looped items", ex.Message));
        }
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<List<ItemDto>>>> GetPending()
    {
        try
        {
            var userGuid = GetUserGuid();
            var items = await _itemService.GetPendingItemsAsync(userGuid);
            
            var itemDtos = await items.ToDtoListAsync(_mapper, 
                id => _categoryService.GetByIdAsync(id));

            return Ok(ApiResponse<List<ItemDto>>.SuccessResult(itemDtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ItemDto>>.ErrorResult("Failed to retrieve pending items", ex.Message));
        }
    }

    [HttpGet("completed")]
    public async Task<ActionResult<ApiResponse<List<ItemDto>>>> GetCompleted()
    {
        try
        {
            var userGuid = GetUserGuid();
            var items = await _itemService.GetCompletedItemsAsync(userGuid);
            
            var itemDtos = await items.ToDtoListAsync(_mapper, 
                id => _categoryService.GetByIdAsync(id));

            return Ok(ApiResponse<List<ItemDto>>.SuccessResult(itemDtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<ItemDto>>.ErrorResult("Failed to retrieve completed items", ex.Message));
        }
    }
}