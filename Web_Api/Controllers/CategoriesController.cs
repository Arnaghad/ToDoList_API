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
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IItemService _itemService;
    private readonly IMapper _mapper;

    public CategoriesController(
        ICategoryService categoryService, 
        IItemService itemService,
        IMapper mapper)
    {
        _categoryService = categoryService;
        _itemService = itemService;
        _mapper = mapper;
    }

    private string GetUserGuid()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new UnauthorizedAccessException("User not authenticated");
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll()
    {
        try
        {
            var userGuid = GetUserGuid();
            var categories = await _categoryService.GetByUserGuidAsync(userGuid);
            var categoryDtos = new List<CategoryDto>();

            foreach (var category in categories)
            {
                var itemsCount = await _categoryService.GetItemsCountAsync(category.Id);
                categoryDtos.Add(category.ToDto(_mapper, itemsCount));
            }

            return Ok(ApiResponse<List<CategoryDto>>.SuccessResult(categoryDtos));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<CategoryDto>>.ErrorResult("Failed to retrieve categories", ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetById(int id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            
            if (category == null)
                return NotFound(ApiResponse<CategoryDto>.ErrorResult("Category not found"));

            if (category.UserGuid != GetUserGuid())
                return Forbid();

            var itemsCount = await _categoryService.GetItemsCountAsync(id);
            var categoryDto = category.ToDto(_mapper, itemsCount);

            return Ok(ApiResponse<CategoryDto>.SuccessResult(categoryDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResult("Failed to retrieve category", ex.Message));
        }
    }

    [HttpGet("{id}/with-items")]
    public async Task<ActionResult<ApiResponse<CategoryWithItemsDto>>> GetWithItems(int id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            
            if (category == null)
                return NotFound(ApiResponse<CategoryWithItemsDto>.ErrorResult("Category not found"));

            if (category.UserGuid != GetUserGuid())
                return Forbid();

            var items = await _itemService.GetByCategoryIdAsync(id);
            var categoryDto = category.ToDetailedDto(_mapper, items);

            return Ok(ApiResponse<CategoryWithItemsDto>.SuccessResult(categoryDto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryWithItemsDto>.ErrorResult(
                "Failed to retrieve category with items", ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var userGuid = GetUserGuid();
            var category = dto.ToEntity(_mapper, userGuid);

            var created = await _categoryService.CreateAsync(category);
            var createdDto = created.ToDto(_mapper);

            return CreatedAtAction(
                nameof(GetById), 
                new { id = created.Id }, 
                ApiResponse<CategoryDto>.SuccessResult(createdDto, "Category created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CategoryDto>.ErrorResult("Category already exists", ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<CategoryDto>.ErrorResult("Validation failed", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResult("Failed to create category", ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            
            if (category == null)
                return NotFound(ApiResponse<CategoryDto>.ErrorResult("Category not found"));

            if (category.UserGuid != GetUserGuid())
                return Forbid();

            dto.UpdateEntity(_mapper, category);
            var updated = await _categoryService.UpdateAsync(id, category);

            if (updated == null)
                return NotFound(ApiResponse<CategoryDto>.ErrorResult("Category not found"));

            var itemsCount = await _categoryService.GetItemsCountAsync(id);
            var updatedDto = updated.ToDto(_mapper, itemsCount);

            return Ok(ApiResponse<CategoryDto>.SuccessResult(updatedDto, "Category updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CategoryDto>.ErrorResult("Category name already exists", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<CategoryDto>.ErrorResult("Failed to update category", ex.Message));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            
            if (category == null)
                return NotFound(ApiResponse<bool>.ErrorResult("Category not found"));

            if (category.UserGuid != GetUserGuid())
                return Forbid();

            var result = await _categoryService.DeleteAsync(id);
            
            return result
                ? Ok(ApiResponse<bool>.SuccessResult(true, "Category deleted successfully"))
                : NotFound(ApiResponse<bool>.ErrorResult("Category not found"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResult(
                "Cannot delete category with items", ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Failed to delete category", ex.Message));
        }
    }

    [HttpDelete("{id}/with-items")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWithItems(int id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            
            if (category == null)
                return NotFound(ApiResponse<bool>.ErrorResult("Category not found"));

            if (category.UserGuid != GetUserGuid())
                return Forbid();

            var result = await _categoryService.DeleteWithItemsAsync(id);
            
            return result
                ? Ok(ApiResponse<bool>.SuccessResult(true, 
                    "Category deleted successfully and items unlinked"))
                : NotFound(ApiResponse<bool>.ErrorResult("Category not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResult(
                "Failed to delete category", ex.Message));
        }
    }

    [HttpGet("{id}/items-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetItemsCount(int id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            
            if (category == null)
                return NotFound(ApiResponse<int>.ErrorResult("Category not found"));

            if (category.UserGuid != GetUserGuid())
                return Forbid();

            var count = await _categoryService.GetItemsCountAsync(id);

            return Ok(ApiResponse<int>.SuccessResult(count));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<int>.ErrorResult(
                "Failed to get items count", ex.Message));
        }
    }

    [HttpGet("check-name")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckNameExists([FromQuery] string name)
    {
        try
        {
            var userGuid = GetUserGuid();
            var exists = await _categoryService.CategoryExistsAsync(userGuid, name);

            return Ok(ApiResponse<bool>.SuccessResult(exists));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResult(
                "Failed to check category name", ex.Message));
        }
    }
}