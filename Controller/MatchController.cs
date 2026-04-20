using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Crazy_Lobby.Services;
using Crazy_Lobby.Models;

namespace Crazy_Lobby.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatchController : BaseController
    {
        private readonly IRoomService _roomService;
        private readonly IUserService _userService;

        public MatchController(IRoomService roomService, IUserService userService)
        {
            _roomService = roomService;
            _userService = userService;
        }

    [HttpGet("{roomId}/players")]
    public async Task<IActionResult> GetPlayersInRoom(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            return BadRequest("Mã phòng (roomId) không được để trống.");
        }

        var playerIds = _roomService.GetPlayerIdsInRoom(roomId);
        var users = await _userService.GetUsersByIdsAsync(playerIds);
        var playersInRoom = users.Select(u => new 
        { 
            u.PlayerId, 
            u.Username 
        });

        return Ok(playersInRoom);
    }

    [HttpPost("result")]
    public IActionResult PostMatchResult([FromBody] MatchResultRequest request)
    {
        if (string.IsNullOrEmpty(CurrentPlayerId)) return Unauthorized();

        // Log match result
        Console.WriteLine($"[MatchResult] Player {CurrentPlayerId} finish room {request.RoomId} | Score: {request.Score} | Combo: {request.MaxCombo}");
        
        return Ok(new { Message = "Kết quả trận đấu đã được ghi nhận!" });
    }
}
}