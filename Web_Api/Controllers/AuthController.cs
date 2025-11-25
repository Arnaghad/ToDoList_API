using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;
using WebApi.DTOs.Auth;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> Register([FromBody] RegisterDto model)
    {
        var result = await _authService.RegisterAsync(model.Email, model.Password, model.Role);

        if (!result.Success)
            return BadRequest(ApiResponse<string>.ErrorResult(result.Message));

        return Ok(ApiResponse<string>.SuccessResult(result.Token!, result.Message));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<string>>> Login([FromBody] LoginDto model)
    {
        var result = await _authService.LoginAsync(model.Email, model.Password);

        if (!result.Success)
            return Unauthorized(ApiResponse<string>.ErrorResult(result.Message));

        return Ok(ApiResponse<string>.SuccessResult(result.Token!, result.Message));
    }
}