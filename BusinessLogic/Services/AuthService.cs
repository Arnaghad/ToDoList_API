using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLogic.Interfaces;
using DataLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AuthUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<AuthUser> userManager, 
        RoleManager<IdentityRole> roleManager, 
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message, string? Token)> RegisterAsync(string email, string password, string role)
    {
        var userExists = await _userManager.FindByEmailAsync(email);
        if (userExists != null)
            return (false, "User already exists", null);

        var user = new AuthUser
        {
            Email = email,
            UserName = email,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)), null);

        // Додаємо роль (якщо вона не існує, створюємо - хоча краще це робити в Seeder)
        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));

        await _userManager.AddToRoleAsync(user, role);

        var token = await GenerateJwtToken(user);
        return (true, "User created successfully", token);
    }

    public async Task<(bool Success, string Message, string? Token)> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            return (false, "Invalid email or password", null);

        var token = await GenerateJwtToken(user);
        return (true, "Login successful", token);
    }

    private async Task<string> GenerateJwtToken(AuthUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        };

        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(authClaims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpireMinutes"]!)),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }
}