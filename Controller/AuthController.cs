using Microsoft.AspNetCore.Mvc;
using Crazy_Lobby.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    [HttpPost("Register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Yêu cầu không hợp lệ: Username và Password không được để trống.");
        }
        Console.WriteLine($"Register request received for User: {request.Username}");
        return Ok(new { Message = "Đăng ký thành công!" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Username))
        {
            return BadRequest("Yêu cầu không hợp lệ: Username không được để trống.");
        }

        Console.WriteLine($"Login request received for User: {request.Username}");

        // Tạo PlayerId một lần duy nhất để đồng bộ giữa Token và Response
        var playerId = Guid.NewGuid().ToString();
        var token = GenerateJWTToken(request.Username, playerId);

        return Ok(new  { Token = token, PlayerId = playerId });
    }
    
    private string GenerateJWTToken(string username, string playerId)
    {
        var SecretKey = "definitely-a-very-secure-secret-key";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim("PlayerId", playerId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID độc nhất của cái token này
        };

        // 3. ĐÚC TOKEN:
        var token = new JwtSecurityToken(
            issuer: "Scrazy_Lobby",        
            audience: "Client",      
            claims: claims,                
            expires: DateTime.Now.AddDays(7), 
            signingCredentials: credentials); 

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}