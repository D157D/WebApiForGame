using Microsoft.AspNetCore.Mvc;
using Crazy_Lobby.Models;
using Microsoft.AspNetCore.Authorization;
using Crazy_Lobby.AppDataContext;
using System.Linq;
using System;

[ApiController]
[Route("api/[controller]")]
#pragma warning disable CA1050 // Declare types in namespaces
public class ChatController(AppDbContext context) : ControllerBase
#pragma warning restore CA1050
{
    private readonly AppDbContext _context = context;

    [Authorize]
    [HttpPost("send")]
    public IActionResult SendMessage([FromBody] SendChatRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Message))
            return BadRequest("Nội dung tin nhắn không được rỗng.");

        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        string? receiverId = null;
        if (!string.IsNullOrEmpty(request.ReceiverUsername))
        {
            var receiver = _context.Users.FirstOrDefault(u => u.Username == request.ReceiverUsername || u.DisplayName == request.ReceiverUsername);
            if (receiver == null) return NotFound("Người nhận không tồn tại.");
            receiverId = receiver.PlayerId;

            var areFriends = _context.Friendships.Any(f => 
                (f.PlayerId1 == currentPlayerId && f.PlayerId2 == receiverId) || 
                (f.PlayerId1 == receiverId && f.PlayerId2 == currentPlayerId));
            
            if (!areFriends) return BadRequest("Chỉ có thể nhắn tin riêng cho bạn bè.");
        }

        var chatMessage = new ChatMessage
        {
            SenderId = currentPlayerId,
            ReceiverId = receiverId,
            Message = request.Message,
            SentAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(chatMessage);
        _context.SaveChanges();

        return Ok(new { Message = "Đã gửi tin nhắn." });
    }

    [Authorize]
    [HttpGet("messages")]
    public IActionResult GetMessages([FromQuery] string? after)
    {
        var currentPlayerId = User.FindFirst("PlayerId")?.Value;
        if (string.IsNullOrEmpty(currentPlayerId)) return Unauthorized();

        var myFriendIds = _context.Friendships
            .Where(f => f.PlayerId1 == currentPlayerId || f.PlayerId2 == currentPlayerId)
            .Select(f => f.PlayerId1 == currentPlayerId ? f.PlayerId2 : f.PlayerId1)
            .ToList();

        var query = _context.ChatMessages
            .Where(m => 
                (m.ReceiverId == null && (m.SenderId == currentPlayerId || myFriendIds.Contains(m.SenderId))) ||
                (m.ReceiverId == currentPlayerId) ||
                (m.SenderId == currentPlayerId && m.ReceiverId != null)
            );

        if (!string.IsNullOrEmpty(after))
        {
            var afterMsg = _context.ChatMessages.FirstOrDefault(m => m.Id == after);
            if (afterMsg != null)
            {
                query = query.Where(m => m.SentAt > afterMsg.SentAt);
            }
        }

        var messages = query.OrderByDescending(m => m.SentAt).Take(50).ToList();
        messages.Reverse();

        var response = messages.Select(m => {
            var senderUser = _context.Users.FirstOrDefault(u => u.PlayerId == m.SenderId);
            var receiverUser = m.ReceiverId != null ? _context.Users.FirstOrDefault(u => u.PlayerId == m.ReceiverId) : null;
            return new ChatMessageData
            {
                Id = m.Id,
                SenderUsername = senderUser?.Username ?? "Unknown",
                SenderDisplayName = senderUser?.DisplayName ?? "Unknown",
                ReceiverUsername = receiverUser?.Username ?? "",
                Message = m.Message,
                SentAt = m.SentAt.ToString("o")
            };
        }).ToArray();

        // Note: the frontend endpoint expects a direct array of ChatMessageData (seen from JSONHelper usage in backendmanager)
        return Ok(response);
    }
}
