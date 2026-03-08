namespace Crazy_Lobby.Models
{
    public class CreateRoomRequest
    {
        public string RoomName { get; set; }
        public int MaxPlayers { get; set; }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}