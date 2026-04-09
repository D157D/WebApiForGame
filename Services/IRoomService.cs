using Crazy_Lobby.Models;
using System.Collections.Generic;

namespace Crazy_Lobby.Services
{
    public interface IRoomService
    {
        string CreateRoom(string roomName, int maxPlayers, string hostId);
        IEnumerable<Room> GetAllRooms();
        Room? GetRoom(string roomId);
        bool JoinRoom(string roomId, string playerId);
        void LeaveRoom(string roomId, string playerId);
        bool IsHost(string roomId, string playerId);
        void RemoveRoom(string roomId);
        IEnumerable<string> GetPlayerIdsInRoom(string roomId);
    }
}