namespace Crazy_Lobby.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public string? SenderId { get; set; }
        public string? ReceiverId { get; set; }
        public string? Status { get; set; } // Trạng thái: "Pending" (đang chờ), "Accepted" (Đã chấp nhận)
    }
}