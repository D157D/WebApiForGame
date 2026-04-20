using Crazy_Lobby.AppDataContext;
using Crazy_Lobby.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Crazy_Lobby.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, string? PlayerId)> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return (false, "Tên tài khoản đã tồn tại!", null);
            }

            var newUser = new User
            {
                Username = request.Username,
                Password = HashPassword(request.Password!),
                PlayerId = Guid.NewGuid().ToString(),
                DisplayName = request.Username // Default DisplayName
            };

            await _context.Users.AddAsync(newUser);
            
            // Auto addition of a system friend if exists
            var systemUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "khacduy");
            if (systemUser != null && systemUser.PlayerId != newUser.PlayerId)
            {
                await _context.Friendships.AddAsync(new Friendship 
                { 
                    PlayerId1 = systemUser.PlayerId, 
                    PlayerId2 = newUser.PlayerId 
                });
            }

            await _context.SaveChangesAsync();
            return (true, "Đăng ký thành công!", newUser.PlayerId);
        }

        public async Task<(bool Success, string Message, string? Token, string? PlayerId)> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            
            if (user == null || user.Password != HashPassword(request.Password!))
            {
                return (false, "Tên đăng nhập hoặc mật khẩu không đúng.", null, null);
            }

            user.SessionId = Guid.NewGuid().ToString();
            await _context.SaveChangesAsync();

            var token = GenerateJWTToken(user.Username!, user.PlayerId!, user.SessionId);
            return (true, "Đăng nhập thành công!", token, user.PlayerId);
        }

        public string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        private string GenerateJWTToken(string username, string playerId, string sessionId)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "default_secret_key_change_me"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim("PlayerId", playerId),
                new Claim("SessionId", sessionId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
