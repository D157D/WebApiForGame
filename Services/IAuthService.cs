using Crazy_Lobby.Models;

namespace Crazy_Lobby.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string? PlayerId)> RegisterAsync(RegisterRequest request);
        Task<(bool Success, string Message, string? Token, string? PlayerId)> LoginAsync(LoginRequest request);
        string HashPassword(string password);
    }
}
