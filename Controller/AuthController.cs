using Microsoft.AspNetCore.Mvc;
using Crazy_Lobby.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Crazy_Lobby.AppDataContext;
using System.Security.Cryptography;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppDbContext _context;

    public AuthController(IHttpClientFactory httpClientFactory, AppDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _context = context;
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
        
        // Kiểm tra xem Username đã tồn tại chưa
        if (_context.Users.Any(u => u.Username == request.Username))
        {
            return BadRequest("Tên tài khoản đã tồn tại!");
        }

        var newUser = new User
        {
            Username = request.Username,
            Password = HashPassword(request.Password),
            PlayerId = Guid.NewGuid().ToString()
        };

        _context.Users.Add(newUser);
        _context.SaveChanges();

        return Ok(new { Message = "Đăng ký thành công!", PlayerId = newUser.PlayerId });
    }

    

    //Login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Yêu cầu không hợp lệ: Username và Password không được để trống.");
        }

        Console.WriteLine($"Login request received for User: {request.Username}");

        var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);

        if (user == null || user.Password != HashPassword(request.Password))
        {
            return Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng.");
        }

        // Tạo một SessionId mới mỗi lần đăng nhập để vô hiệu hóa các token cũ
        user.SessionId = Guid.NewGuid().ToString();
        _context.SaveChanges();

        var token = GenerateJWTToken(user.Username, user.PlayerId, user.SessionId);

        return Ok(new  { Token = token, PlayerId = user.PlayerId });
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
    private string GenerateJWTToken(string username, string playerId, string sessionId)
    {
        var SecretKey = "definitely-a-very-secure-secret-key";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim("PlayerId", playerId),
            new Claim("SessionId", sessionId),
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

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}