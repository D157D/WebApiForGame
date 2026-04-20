using Microsoft.AspNetCore.Mvc;
using Crazy_Lobby.Models;
using Crazy_Lobby.Services;
using Microsoft.AspNetCore.Authorization;

namespace Crazy_Lobby.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Yêu cầu không hợp lệ: Username và Password không được để trống.");
        }

        var (success, message, playerId) = await _authService.RegisterAsync(request);
        if (!success) return BadRequest(message);

        return Ok(new { Message = message, PlayerId = playerId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Yêu cầu không hợp lệ: Username và Password không được để trống.");
        }

        var (success, message, token, playerId) = await _authService.LoginAsync(request);
        if (!success) return Unauthorized(message);

        return Ok(new { Token = token, PlayerId = playerId });
    }

    [Authorize]
    [HttpPost("add-friend")]
    public async Task<IActionResult> AddFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }

        var (success, message) = await _userService.AddFriendAsync(CurrentPlayerId!, request.FriendUsername);
        if (!success) return BadRequest(message);

        return Ok(new { Message = message });
    }

    [Authorize]
    [HttpPost("accept-friend")]
    public async Task<IActionResult> AcceptFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }

        var (success, message) = await _userService.AcceptFriendAsync(CurrentPlayerId!, request.FriendUsername);
        if (!success) return BadRequest(message);

        return Ok(new { Message = message });
    }

    [Authorize]
    [HttpPost("decline-friend")]
    public async Task<IActionResult> DeclineFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }

        var (success, message) = await _userService.DeclineFriendAsync(CurrentPlayerId!, request.FriendUsername);
        if (!success) return BadRequest(message);

        return Ok(new { Message = message });
    }

    [Authorize]
    [HttpPost("invite-game")]
    public async Task<IActionResult> InviteFriend([FromBody] GameInviteRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername) || string.IsNullOrEmpty(request.RoomId))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername và RoomId không được để trống.");
        }

        var (success, message, inviteId) = await _userService.InviteFriendAsync(CurrentPlayerId!, request.FriendUsername, request.RoomId);
        if (!success) return BadRequest(message);

        return Ok(new { Message = message, InviteId = inviteId });
    }

    [Authorize]
    [HttpGet("get-invites")]
    public async Task<IActionResult> GetGameInvites()
    {
        var invites = await _userService.GetGameInvitesAsync(CurrentPlayerId!);
        return Ok(invites);
    }

    [Authorize]
    [HttpPost("respond-invite")]
    public async Task<IActionResult> RespondToGameInvite([FromBody] RespondInviteRequest request)
    {
        var (success, message, status, roomId) = await _userService.RespondToGameInviteAsync(CurrentPlayerId!, request.InviteId, request.Status!);
        if (!success) return BadRequest(message);

        return Ok(new { Message = message, Status = status, RoomId = roomId });
    }

    [Authorize]
    [HttpDelete("delete-friend")]
    public async Task<IActionResult> DeleteFriend([FromBody] AddFriend request)
    {
        if (request == null || string.IsNullOrEmpty(request.FriendUsername))
        {
            return BadRequest("Yêu cầu không hợp lệ: FriendUsername không được để trống.");
        }

        var (success, message) = await _userService.DeleteFriendAsync(CurrentPlayerId!, request.FriendUsername);
        if (!success) return BadRequest(message);

        return Ok(new { Message = message });
    }

    [Authorize]
    [HttpGet("get-friends")]
    public async Task<IActionResult> GetFriends()
    {
        var friends = await _userService.GetFriendsAsync(CurrentPlayerId!);
        return Ok(friends);
    }

    [Authorize]
    [HttpGet("get-friend-requests")]
    public async Task<IActionResult> GetFriendRequests()
    {
        var requests = await _userService.GetFriendRequestsAsync(CurrentPlayerId!);
        return Ok(requests);
    }
}
}