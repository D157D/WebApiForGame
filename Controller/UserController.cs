using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Crazy_Lobby.Models;
using Crazy_Lobby.AppDataContext;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy thông tin profile của người chơi hiện tại (username + displayName).
    /// GET /api/User/profile
    /// </summary>
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var playerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(playerId)) return Unauthorized("Token không hợp lệ.");

        var user = _context.Users.FirstOrDefault(u => u.PlayerId == playerId);
        if (user == null) return NotFound("Người dùng không tồn tại.");

        var response = new UserProfileResponse
        {
            Username = user.Username,
            // Nếu chưa đặt displayName, trả về username làm mặc định
            DisplayName = string.IsNullOrEmpty(user.DisplayName) ? user.Username : user.DisplayName
        };

        return Ok(response);
    }

    /// <summary>
    /// Đổi tên hiển thị trong game mà không ảnh hưởng tên đăng nhập.
    /// PUT /api/User/display-name
    /// </summary>
    [HttpPut("display-name")]
    public IActionResult UpdateDisplayName([FromBody] UpdateDisplayNameRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest("Tên hiển thị không được để trống.");
        }

        // Giới hạn độ dài tên hiển thị
        if (request.DisplayName.Length < 2 || request.DisplayName.Length > 20)
        {
            return BadRequest("Tên hiển thị phải từ 2 đến 20 ký tự.");
        }

        var playerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(playerId)) return Unauthorized("Token không hợp lệ.");

        var user = _context.Users.FirstOrDefault(u => u.PlayerId == playerId);
        if (user == null) return NotFound("Người dùng không tồn tại.");

        // Kiểm tra trùng tên hiển thị với người khác
        var isDuplicate = _context.Users.Any(u => u.DisplayName == request.DisplayName && u.PlayerId != playerId);
        if (isDuplicate)
        {
            return BadRequest("Tên hiển thị này đã được sử dụng bởi người chơi khác.");
        }

        user.DisplayName = request.DisplayName;
        _context.SaveChanges();

        Console.WriteLine($"User {user.Username} đổi tên hiển thị thành: {request.DisplayName}");

        return Ok(new { Message = "Đổi tên hiển thị thành công!", DisplayName = user.DisplayName });
    }

    /// <summary>
    /// Tìm kiếm user theo Tên đăng nhập.
    /// GET /api/User/search?query=...
    /// </summary>
    [HttpGet("search")]
    public IActionResult SearchUsers([FromQuery] string query)
    {
        var playerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(playerId)) return Unauthorized("Token không hợp lệ.");

        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(new List<FriendResponse>());
        }

        var users = _context.Users
            .Where(u => u.PlayerId != playerId && (u.Username.Contains(query) || (u.DisplayName != null && u.DisplayName.Contains(query))))
            .Take(20)
            .Select(u => new FriendResponse
            {
                Username = u.Username,
                DisplayName = string.IsNullOrEmpty(u.DisplayName) ? u.Username : u.DisplayName,
                Status = "Online", // Tạm thời để online
                CharacterType = "default"
            })
            .ToList();

        return Ok(users);
    }
}
