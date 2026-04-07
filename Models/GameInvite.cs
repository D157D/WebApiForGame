using System;

namespace Crazy_Lobby.Models
{
    public class GameInvite
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty; // Session ID hoặc Room Name
        public string Status { get; set; } = "Pending"; // "Pending", "Accepted", "Declined", "Expired"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
