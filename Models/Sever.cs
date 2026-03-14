namespace Crazy_Lobby.Models
{
    public class Player
    {
        public string? PlayerId { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
    }

    public class Room
    {
        public string? RoomId { get; set; }
        public string? HostId { get; set; }
        public string? RoomName { get; set; }
        public int MaxPlayers { get; set; }
        public bool isPlaying { get; set; }
    }

    public class MatchResult
    {
        public string? MatchId { get; set; }
        public string? RoomId { get; set; }
        public string? PlayerId { get; set; }
        public float FinishTime { get; set; }
        public int Placement { get; set; }
    }
}