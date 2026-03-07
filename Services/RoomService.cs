using System.Collections.Generic;
using System.Linq;
using Sever.Models;

namespace Crazy_Lobby.Services
{
    public class RoomService : IRoomService
    {
        // Lưu trữ danh sách phòng trong RAM
        private readonly List<Room> _rooms = new List<Room>();

        public void AddRoom(Room room)
        {
            _rooms.Add(room);
        }

        public IEnumerable<Room> GetWaitingRooms()
        {
            // Trả về các phòng chưa bắt đầu chơi (isPlaying == false)
            return _rooms.Where(r => !r.isPlaying).ToList();
        }

        public bool IsHost(string roomId, string playerId)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            return room != null && room.HostId == playerId;
        }

        public void RemoveRoom(string roomId)
        {
            var room = _rooms.FirstOrDefault(r => r.RoomId == roomId);
            if (room != null)
            {
                _rooms.Remove(room);
            }
        }
    }
}