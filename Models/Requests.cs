namespace Crazy_Lobby.Models
{
    public class CreateRoomRequest
    {
        public string? RoomName { get; set; }
        public int MaxPlayers { get; set; }
    }

    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class RegisterRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
    public class AddFriend()
    {
        public string? FriendUsername { get; set; }
        public string? PlayerId { get; set; }
        public string? FriendId { get; set; }
        public string? Message { get; set; }
    }

    // Request đổi tên hiển thị
    public class UpdateDisplayNameRequest
    {
        public string? DisplayName { get; set; }
    }

    // Response trả về thông tin profile của người chơi
    public class UserProfileResponse
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}