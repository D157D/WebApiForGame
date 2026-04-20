using Crazy_Lobby.Models;
using Crazy_Lobby.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Crazy_Lobby.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomController : BaseController
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

    [HttpPost("create")]
    public IActionResult CreateRoom([FromBody] CreateRoomRequest request)
    {
        if (string.IsNullOrEmpty(CurrentPlayerId)) return Unauthorized();

        var roomId = _roomService.CreateRoom(request.RoomName ?? "New Room", request.MaxPlayers, CurrentPlayerId);
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
        if (string.IsNullOrEmpty(CurrentPlayerId)) return Unauthorized();

        if (_roomService.JoinRoom(roomId, CurrentPlayerId))
        {
            return Ok(new { Message = "Joined successfully" });
        }
        return BadRequest("Room full or not found");
    }

    [HttpPost("leave")]
    public IActionResult LeaveRoom([FromBody] string roomId)
    {
        if (string.IsNullOrEmpty(CurrentPlayerId)) return Unauthorized();

        _roomService.LeaveRoom(roomId, CurrentPlayerId);
        return Ok(new { Message = "Left successfully" });
    }
}

}