using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Crazy_Lobby.Models;
using Crazy_Lobby.Services;

namespace Crazy_Lobby.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        if (string.IsNullOrEmpty(CurrentPlayerId)) return Unauthorized("Token không hợp lệ.");

        var response = await _userService.GetProfileAsync(CurrentPlayerId);
        if (response == null) return NotFound("Người dùng không tồn tại.");

        return Ok(response);
    }

    [HttpPut("display-name")]
    public async Task<IActionResult> UpdateDisplayName([FromBody] UpdateDisplayNameRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest("Tên hiển thị không được để trống.");
        }

        if (request.DisplayName.Length < 2 || request.DisplayName.Length > 20)
        {
            return BadRequest("Tên hiển thị phải từ 2 đến 20 ký tự.");
        }

        if (string.IsNullOrEmpty(CurrentPlayerId)) return Unauthorized("Token không hợp lệ.");

        var (success, message, displayName) = await _userService.UpdateDisplayNameAsync(CurrentPlayerId, request.DisplayName);
        if (!success) return BadRequest(message);

        return Ok(new { Message = message, DisplayName = displayName });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        if (string.IsNullOrEmpty(CurrentPlayerId)) return Unauthorized("Token không hợp lệ.");

        var users = await _userService.SearchUsersAsync(CurrentPlayerId, query);
        return Ok(users);
    }
}
}