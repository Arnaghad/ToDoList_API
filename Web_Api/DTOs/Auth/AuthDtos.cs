using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;
    
    // Опціонально, якщо ви хочете дозволити реєстрацію адмінів одразу (для тестування)
    // У реальному проєкті це поле зазвичай прибирають
    public string Role { get; set; } = "User"; 
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
}