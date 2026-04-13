namespace Crazy_Lobby.Models
{
    public class CreateRoomRequest
    {
        public string? RoomName { get; set; }
        public int MaxPlayers { get; set; }
    }

    public class CreateRoomResponse
    {
        public string? RoomId { get; set; }
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

    public class MatchResultRequest
    {
        public string? RoomId { get; set; }
        public int Score { get; set; }
        public int MaxCombo { get; set; }
        public int PerfectHits { get; set; }
        public int MissHits { get; set; }
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

    public class GameInviteRequest
    {
        public string? FriendUsername { get; set; }
        public string? RoomId { get; set; }
    }

    public class RespondInviteRequest
    {
        public int InviteId { get; set; }
        public string? Status { get; set; } // "Accepted" or "Declined"
    }

    public class GameInviteResponse
    {
        public int InviteId { get; set; }
        public string? SenderUsername { get; set; }
        public string? SenderDisplayName { get; set; }
        public string? RoomId { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FriendResponse
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Status { get; set; } = "Online";
        public string CharacterType { get; set; } = "default";
    }

    public class FriendRequestResponse
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string CharacterType { get; set; } = "default";
    }

    public class SendChatRequest
    {
        public string? Message { get; set; }
        public string? ReceiverUsername { get; set; }
    }

    public class ChatMessageData
    {
        public string? Id { get; set; }
        public string? SenderUsername { get; set; }
        public string? SenderDisplayName { get; set; }
        public string? ReceiverUsername { get; set; }
        public string? Message { get; set; }
        public string? SentAt { get; set; }
    }
}