using Sever.Models;

namespace Crazy_Lobby.Services
{
    public interface IRoomService
    {
        void AddRoom(Room room);
        IEnumerable<Room> GetWaitingRooms();
        bool IsHost(string roomId, string playerId);
        void RemoveRoom(string roomId);
    }
}