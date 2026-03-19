using System.Collections.Generic;

namespace Crazy_Lobby.Services
{
    public interface IRoomService
    {
        bool IsHost(string roomId, string playerId);
        void RemoveRoom(string roomId);
        IEnumerable<string> GetPlayerIdsInRoom(string roomId);
    }
}