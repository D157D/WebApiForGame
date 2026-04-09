using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crazy_Lobby.Services;
using Crazy_Lobby.AppDataContext;
using Crazy_Lobby.Models;
using System.Linq;
using System.Security.Claims;

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

    [HttpGet("{roomId}/players")]
    public IActionResult GetPlayersInRoom(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            return BadRequest("Mã phòng (roomId) không được để trống.");
        }

        // Lấy danh sách PlayerId đang ở trong phòng từ RoomService
        var playerIds = _roomService.GetPlayerIdsInRoom(roomId);
        
        if (playerIds == null || !playerIds.Any())
        {
            return NotFound("Phòng không tồn tại hoặc không có người chơi nào.");
        }

        // Truy vấn Database để mapping PlayerId ra Username
        var playersInRoom = _dbContext.Users
            .Where(u => playerIds.Contains(u.PlayerId))
            .Select(u => new 
            { 
                u.PlayerId, 
                u.Username 
            })
            .ToList();

        return Ok(playersInRoom);
    }

    [HttpPost("result")]
    public IActionResult PostMatchResult([FromBody] MatchResultRequest request)
    {
        var playerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(playerId)) return Unauthorized();

        // Log match result
        Console.WriteLine($"[MatchResult] Player {playerId} finish room {request.RoomId} | Score: {request.Score} | Combo: {request.MaxCombo}");
        
        return Ok(new { Message = "Kết quả trận đấu đã được ghi nhận!" });
    }
}