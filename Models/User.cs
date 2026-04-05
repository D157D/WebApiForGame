using System.ComponentModel.DataAnnotations;

namespace Crazy_Lobby.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public string SessionId {get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty; // Tên hiển thị trong game, tách biệt với Username
    }
}