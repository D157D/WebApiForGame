using Microsoft.AspNetCore.Mvc;
using Crazy_Lobby.Models;
using Sever.Models;
using Crazy_Lobby.Services;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost("create-room")]
    public IActionResult CreateRoom([FromBody] CreateRoomRequest request)
    {
       var hostId = User.FindFirst("PlayerId")?.Value; 
        
        // Tạo RoomID ngẫu nhiên (hoặc dùng Session Name của Fusion)
        var roomId = Guid.NewGuid().ToString();

#pragma warning disable CS8601 // Possible null reference assignment.
        var newRoom = new Room { 
            RoomId = roomId, 
            HostId = hostId, 
            RoomName = request.RoomName,
            MaxPlayers = request.MaxPlayers,
            isPlaying = false 
        };
#pragma warning restore CS8601 // Possible null reference assignment.
                              // Lưu newRoom vào RAM hoặc Redis...
        _roomService.AddRoom(newRoom);

        return Ok(new { RoomId = roomId });
    }       
    [HttpGet("rooms")]
    public IActionResult GetAvailableRooms()
    {
        // Lấy danh sách các phòng có IsPlaying == false
        var rooms = _roomService.GetWaitingRooms(); 
        return Ok(rooms);
    }
}