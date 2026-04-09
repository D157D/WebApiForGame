using Crazy_Lobby.Models;
using Crazy_Lobby.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
#pragma warning disable CA1050
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost("create")]
    public IActionResult CreateRoom([FromBody] CreateRoomRequest request)
    {
        var playerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(playerId)) return Unauthorized();

        var roomId = _roomService.CreateRoom(request.RoomName ?? "New Room", request.MaxPlayers, playerId);
        return Ok(new CreateRoomResponse { RoomId = roomId });
    }

    [HttpGet("list")]
    public IActionResult GetRooms()
    {
        var rooms = _roomService.GetAllRooms();
        return Ok(rooms);
    }

    [HttpPost("join")]
    public IActionResult JoinRoom([FromBody] string roomId)
    {
        var playerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(playerId)) return Unauthorized();

        if (_roomService.JoinRoom(roomId, playerId))
        {
            return Ok(new { Message = "Joined successfully" });
        }
        return BadRequest("Room full or not found");
    }

    [HttpPost("leave")]
    public IActionResult LeaveRoom([FromBody] string roomId)
    {
        var playerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(playerId)) return Unauthorized();

        _roomService.LeaveRoom(roomId, playerId);
        return Ok(new { Message = "Left successfully" });
    }
}
#pragma warning restore CA1050
