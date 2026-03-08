using Crazy_Lobby.Data;
using Crazy_Lobby.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sever.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchController : ControllerBase
{
    private readonly IRoomService _roomService;
    private readonly AppDbContext _dbContext;

    public MatchController(IRoomService roomService, AppDbContext dbContext)
    {
        _roomService = roomService;
        _dbContext = dbContext;
    }

    [HttpPost("match-result")]
    public IActionResult SubmitResult([FromBody] MatchResult result)
    {
        var senderId = User.FindFirst("PlayerId")?.Value;

        // Server Validation: Phải đảm bảo người gửi request thực sự là Host của phòng đó
#pragma warning disable CS8604 // Possible null reference argument.
        if (!_roomService.IsHost(result.RoomId, senderId))
        {
            // Forbid() không nhận string message, dùng StatusCode 403 thay thế
            return StatusCode(403, "Chỉ Host mới được quyền gửi kết quả trận đấu.");
        }
#pragma warning restore CS8604 // Possible null reference argument.

        // Lưu kết quả vào Database qua Entity Framework
        _dbContext.MatchResults.Add(result);
        _dbContext.SaveChanges();

        // Xóa phòng khỏi danh sách
        _roomService.RemoveRoom(result.RoomId);

        return Ok(new { Message = "Lưu kết quả thành công!" });
    }
}