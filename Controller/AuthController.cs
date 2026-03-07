using Microsoft.AspNetCore.Mvc;
using Crazy_Lobby.Models;
using System.Net.Http;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var token = "generated-jwt-token"; // Replace with actual token generation logic
        var PlayerId = "player-id"; // Replace with actual player ID retrieval logic
        return Ok(new  { Token = token, PlayerId = PlayerId });
    }
    
    // Ví dụ: Server gọi sang Google (giống UnityWebRequest)
    [HttpGet("test-connection-out")]
    public async Task<IActionResult> TestCallExternal()
    {
        // Tạo client
        var client = _httpClientFactory.CreateClient();
        
        // Gửi Request (GetAsync tương đương UnityWebRequest.Get)
        var response = await client.GetAsync("https://www.google.com");
        
        return Ok(new { 
            Target = "Google", 
            StatusCode = response.StatusCode, 
            IsSuccess = response.IsSuccessStatusCode 
        });
    }
}