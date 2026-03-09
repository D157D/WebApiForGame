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

    //Register
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Yêu cầu không hợp lệ: Username và Password không được để trống.");
        }
        Console.WriteLine($"Register request received for User: {request.Username}");
        return Ok(new { Message = "Đăng ký thành công!" });
    }

    //Login
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

    //AddFriend
    [HttpPost("add-friend")]
    public IActionResult AddFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }
        Console.WriteLine($"AddFriend request received for User: {request.FriendUsername}");

        return Ok(new { Message = $"Yêu cầu kết bạn đã được gửi đến {request.FriendUsername}!" });
    }

    //AcceptFriend
    [HttpPost("accept-friend")]
    public IActionResult AcceptFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }
        Console.WriteLine($"AcceptFriend request received for User: {request.FriendUsername}");

        return Ok(new { Message = $"Bạn đã chấp nhận yêu cầu kết bạn từ {request.FriendUsername}!" });
    }

    //RejectFriend

    [HttpPost("reject-friend")]
    public IActionResult RejectFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }
        Console.WriteLine($"RejectFriend request received for User: {request.FriendUsername}");

        return Ok(new { Message = $"Bạn đã từ chối yêu cầu kết bạn từ {request.FriendUsername}!" });
    }

    //CreateRoom
    [HttpPost("create-room")]
    public IActionResult CreateRoom([FromBody] CreateRoomRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.RoomName) || request.MaxPlayers <= 0)
        {
            return BadRequest("Yêu cầu không hợp lệ: RoomName không được để trống và MaxPlayers phải lớn hơn 0.");
        }
        Console.WriteLine($"CreateRoom request received for Room: {request.RoomName} with MaxPlayers: {request.MaxPlayers}");

        return Ok(new { Message = $"Phòng '{request.RoomName}' đã được tạo với sức chứa tối đa {request.MaxPlayers} người chơi!" });
    }

    //InviteFriend
    [HttpPost("InviteFriend")]
    public IActionResult InviteFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }
        Console.WriteLine($"InviteFriend request received for User: {request.FriendUsername}");

        return Ok(new { Message = $"Bạn đã gửi lời mời chơi đến {request.FriendUsername}!" });
    }

    [HttpDelete("delete-friend")]
    public IActionResult DeleteFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }
        Console.WriteLine($"DeleteFriend request received for User: {request.FriendUsername}");

        return Ok(new { Message = $"Bạn đã xóa {request.FriendUsername} khỏi danh sách bạn bè!" });
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