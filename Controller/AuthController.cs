using Microsoft.AspNetCore.Mvc;
using Crazy_Lobby.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Crazy_Lobby.AppDataContext;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
#pragma warning disable CA1050 // Declare types in namespaces
public class AuthController(IHttpClientFactory httpClientFactory, AppDbContext context) : ControllerBase
#pragma warning restore CA1050 // Declare types in namespaces
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly AppDbContext _context = context;

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
            PlayerId = Guid.NewGuid().ToString(),
            DisplayName = request.Username // Mặc định tên hiển thị = tên đăng nhập
        };

        _context.Users.Add(newUser);
        _context.SaveChanges();

        // Tự động kết bạn với Crazy_Lobby (nếu có)
        var systemUser = _context.Users.FirstOrDefault(u => u.Username == "Crazy_Lobby");
        if (systemUser != null && systemUser.PlayerId != newUser.PlayerId)
        {
            _context.Friendships.Add(new Friendship 
            { 
                PlayerId1 = systemUser.PlayerId, 
                PlayerId2 = newUser.PlayerId 
            });
            _context.SaveChanges();
        }

        return Ok(new { Message = "Đăng ký thành công!", newUser.PlayerId });
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

        return Ok(new  { Token = token, user.PlayerId });
    }

    //AddFriend
    [Authorize]
    [HttpPost("add-friend")]
    public IActionResult AddFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }

        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        var receiver = _context.Users.FirstOrDefault(u => u.Username == request.FriendUsername);
        if (receiver == null) return NotFound("Người dùng không tồn tại.");
        
        if (receiver.PlayerId == currentPlayerId) return BadRequest("Không thể kết bạn với chính mình.");

        // Kiểm tra xem đã là bạn bè chưa
        var isAlreadyFriend = _context.Friendships.Any(f => 
            (f.PlayerId1 == currentPlayerId && f.PlayerId2 == receiver.PlayerId) || 
            (f.PlayerId1 == receiver.PlayerId && f.PlayerId2 == currentPlayerId));
            
        if (isAlreadyFriend) return BadRequest("Hai người đã là bạn bè.");

        // Kiểm tra xem yêu cầu từ mình đến người kia đã tồn tại chưa
        var existingRequest = _context.FriendRequests.FirstOrDefault(f => 
            f.SenderId == currentPlayerId && f.ReceiverId == receiver.PlayerId && f.Status == "Pending");
            
        if (existingRequest != null) return BadRequest("Yêu cầu kết bạn đã được gửi trước đó.");

        // Kiểm tra xem người kia có đang gửi yêu cầu cho mình không
        var reverseRequest = _context.FriendRequests.FirstOrDefault(f => 
            f.SenderId == receiver.PlayerId && f.ReceiverId == currentPlayerId && f.Status == "Pending");
            
        if (reverseRequest != null) return BadRequest("Người này đã gửi yêu cầu kết bạn cho bạn, vui lòng kiểm tra lời mời.");

        _context.FriendRequests.Add(new FriendRequest 
        { 
            SenderId = currentPlayerId, 
            ReceiverId = receiver.PlayerId, 
            Status = "Pending" 
        });
        _context.SaveChanges();

        return Ok(new { Message = $"Yêu cầu kết bạn đã được gửi đến {request.FriendUsername}!" });
    }

    //AcceptFriend
    [Authorize]
    [HttpPost("accept-friend")]
    public IActionResult AcceptFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }

        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        var sender = _context.Users.FirstOrDefault(u => u.Username == request.FriendUsername);
        if (sender == null) return NotFound("Người dùng không tồn tại.");

        var friendRequest = _context.FriendRequests.FirstOrDefault(f => 
            f.SenderId == sender.PlayerId && f.ReceiverId == currentPlayerId && f.Status == "Pending");

        if (friendRequest == null) return NotFound("Không tìm thấy yêu cầu kết bạn đang chờ.");

        friendRequest.Status = "Accepted";

        _context.Friendships.Add(new Friendship 
        { 
            PlayerId1 = sender.PlayerId, 
            PlayerId2 = currentPlayerId 
        });

        _context.SaveChanges();

        return Ok(new { Message = $"Bạn đã chấp nhận yêu cầu kết bạn từ {request.FriendUsername}!" });
    }

    //RejectFriend
    [Authorize]
    [HttpPost("reject-friend")]
    public IActionResult RejectFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }

        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        var sender = _context.Users.FirstOrDefault(u => u.Username == request.FriendUsername);
        if (sender == null) return NotFound("Người dùng không tồn tại.");

        var friendRequest = _context.FriendRequests.FirstOrDefault(f => 
            f.SenderId == sender.PlayerId && f.ReceiverId == currentPlayerId && f.Status == "Pending");

        if (friendRequest == null) return NotFound("Không tìm thấy yêu cầu kết bạn đang chờ.");

        // Từ chối thì xoá luôn bản ghi Request đó
        _context.FriendRequests.Remove(friendRequest);
        _context.SaveChanges();

        return Ok(new { Message = $"Bạn đã từ chối yêu cầu kết bạn từ {request.FriendUsername}!" });
    }
    
    //InviteFriend
    [Authorize]
    [HttpPost("invite-game")]
    public IActionResult InviteFriend([FromBody] GameInviteRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername) || string.IsNullOrEmpty(request.RoomId))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername và RoomId không được để trống.");
        }

        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        var receiver = _context.Users.FirstOrDefault(u => u.Username == request.FriendUsername);
        if (receiver == null) return NotFound("Người dùng không tồn tại.");

        if (receiver.PlayerId == currentPlayerId) return BadRequest("Không thể mời chính mình.");

        // Kiểm tra xem đã là bạn bè chưa (Tùy chọn: có thể cho phép mời cả người không phải bạn)
        var areFriends = _context.Friendships.Any(f => 
            (f.PlayerId1 == currentPlayerId && f.PlayerId2 == receiver.PlayerId) || 
            (f.PlayerId1 == receiver.PlayerId && f.PlayerId2 == currentPlayerId));
            
        if (!areFriends) return BadRequest("Bạn chỉ có thể mời bạn bè chơi cùng.");

        // Xóa lời mời cũ (nếu có) đến cùng người đó cho cùng phòng để tránh spam
        var oldInvite = _context.GameInvites.FirstOrDefault(i => 
            i.SenderId == currentPlayerId && i.ReceiverId == receiver.PlayerId && i.RoomId == request.RoomId && i.Status == "Pending");
        if (oldInvite != null) _context.GameInvites.Remove(oldInvite);

        var invite = new GameInvite
        {
            SenderId = currentPlayerId,
            ReceiverId = receiver.PlayerId,
            RoomId = request.RoomId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.GameInvites.Add(invite);
        _context.SaveChanges();

        return Ok(new { Message = $"Bạn đã gửi lời mời chơi đến {request.FriendUsername}!", InviteId = invite.Id });
    }

    [Authorize]
    [HttpGet("get-game-invites")]
    public IActionResult GetGameInvites()
    {
        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        var invites = _context.GameInvites
            .Where(i => i.ReceiverId == currentPlayerId && i.Status == "Pending")
            .OrderByDescending(i => i.CreatedAt)
            .ToList();

        var response = invites.Select(i => {
            var sender = _context.Users.FirstOrDefault(u => u.PlayerId == i.SenderId);
            return new GameInviteResponse
            {
                InviteId = i.Id,
                SenderUsername = sender?.Username,
                SenderDisplayName = sender?.DisplayName,
                RoomId = i.RoomId,
                Status = i.Status,
                CreatedAt = i.CreatedAt
            };
        });

        return Ok(response);
    }

    [Authorize]
    [HttpPost("respond-invite")]
    public IActionResult RespondToGameInvite([FromBody] RespondInviteRequest request)
    {
        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        var invite = _context.GameInvites.FirstOrDefault(i => i.Id == request.InviteId && i.ReceiverId == currentPlayerId);
        if (invite == null) return NotFound("Không tìm thấy lời mời.");

        if (request.Status != "Accepted" && request.Status != "Declined")
            return BadRequest("Trạng thái không hợp lệ.");

        invite.Status = request.Status;
        _context.SaveChanges();

        string message = request.Status == "Accepted" ? "Bạn đã chấp nhận lời mời." : "Bạn đã từ chối lời mời.";
        return Ok(new { Message = message, invite.Status, invite.RoomId });
    }

    [Authorize]
    [HttpDelete("delete-friend")]
    public IActionResult DeleteFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }

        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        var friend = _context.Users.FirstOrDefault(u => u.Username == request.FriendUsername);
        if (friend == null) return NotFound("Người dùng không tồn tại.");

        // Tìm Friendship không quan trọng thứ tự Player1 và Player2
        var friendship = _context.Friendships.FirstOrDefault(f => 
            (f.PlayerId1 == currentPlayerId && f.PlayerId2 == friend.PlayerId) || 
            (f.PlayerId1 == friend.PlayerId && f.PlayerId2 == currentPlayerId));

        if (friendship == null) return NotFound("Hai người không phải là bạn bè.");

        _context.Friendships.Remove(friendship);
        _context.SaveChanges();

        return Ok(new { Message = $"Bạn đã xóa {request.FriendUsername} khỏi danh sách bạn bè!" });
    }

    [Authorize]
    [HttpGet("get-friends")]
    public IActionResult GetFriends()
    {
        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        Console.WriteLine($"[GetFriends] CurrentPlayerId: {currentPlayerId}");

        var friendships = _context.Friendships
            .Where(f => f.PlayerId1 == currentPlayerId || f.PlayerId2 == currentPlayerId)
            .ToList();

        var friends = new List<FriendResponse>();

        foreach (var friendship in friendships)
        {
            var friendId = friendship.PlayerId1 == currentPlayerId ? friendship.PlayerId2 : friendship.PlayerId1;
            var friendUser = _context.Users.FirstOrDefault(u => u.PlayerId == friendId);
            
            if (friendUser != null)
            {
                friends.Add(new FriendResponse
                {
                    Username = friendUser.Username,
                    DisplayName = string.IsNullOrEmpty(friendUser.DisplayName) ? friendUser.Username : friendUser.DisplayName,
                    Status = "Online",
                    CharacterType = "default"
                });
            }
        }

        Console.WriteLine($"[GetFriends] Returning {friends.Count} friends.");
        return Ok(friends);
    }

    [Authorize]
    [HttpGet("get-pending-requests")]
    public IActionResult GetPendingRequests()
    {
        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        Console.WriteLine($"[GetPendingRequests] CurrentPlayerId: {currentPlayerId}");

        var friendRequests = _context.FriendRequests
            .Where(f => f.ReceiverId == currentPlayerId && f.Status == "Pending")
            .ToList();

        var requests = new List<FriendRequestResponse>();

        foreach (var fr in friendRequests)
        {
            var senderUser = _context.Users.FirstOrDefault(u => u.PlayerId == fr.SenderId);
            if (senderUser != null)
            {
                requests.Add(new FriendRequestResponse
                {
                    SenderUsername = senderUser.Username,
                    SenderDisplayName = string.IsNullOrEmpty(senderUser.DisplayName) ? senderUser.Username : senderUser.DisplayName,
                    CharacterType = "default"
                });
            }
        }

        Console.WriteLine($"[GetPendingRequests] Returning {requests.Count} requests.");
        return Ok(requests);
    }
    private static string GenerateJWTToken(string username, string playerId, string sessionId)
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

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));

        var builder = new StringBuilder();
        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}