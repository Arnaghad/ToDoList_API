using DataLayer.Entities;

namespace BusinessLogic.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string Message, string? Token)> RegisterAsync(string email, string password, string role);
    Task<(bool Success, string Message, string? Token)> LoginAsync(string email, string password);
}