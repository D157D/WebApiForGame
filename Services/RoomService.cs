using Crazy_Lobby.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Crazy_Lobby.Services
{
    public class RoomService : IRoomService
    {
        // Sử dụng ConcurrentDictionary để quản lý phòng trong bộ nhớ
        private static readonly ConcurrentDictionary<string, Room> _rooms = new ConcurrentDictionary<string, Room>();
        private static readonly ConcurrentDictionary<string, List<string>> _roomPlayers = new ConcurrentDictionary<string, List<string>>();

        public string CreateRoom(string roomName, int maxPlayers, string hostId)
        {
            string roomId = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            var room = new Room
            {
                RoomId = roomId,
                RoomName = roomName,
                MaxPlayers = maxPlayers,
                HostId = hostId,
                isPlaying = false
            };

            _rooms[roomId] = room;
            _roomPlayers[roomId] = new List<string> { hostId };
            
            return roomId;
        }

        public IEnumerable<Room> GetAllRooms()
        {
            return _rooms.Values;
        }

        public Room? GetRoom(string roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }

        public bool JoinRoom(string roomId, string playerId)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                if (!_roomPlayers.ContainsKey(roomId))
                {
                    _roomPlayers[roomId] = new List<string>();
                }

                if (_roomPlayers[roomId].Count < room.MaxPlayers)
                {
                    if (!_roomPlayers[roomId].Contains(playerId))
                    {
                        _roomPlayers[roomId].Add(playerId);
                    }
                    return true;
                }
            }
            return false;
        }

        public void LeaveRoom(string roomId, string playerId)
        {
            if (_roomPlayers.TryGetValue(roomId, out var players))
            {
                players.Remove(playerId);
                if (players.Count == 0)
                {
                    RemoveRoom(roomId);
                }
                else if (_rooms.TryGetValue(roomId, out var room) && room.HostId == playerId)
                {
                    // Chuyển host cho người tiếp theo
                    room.HostId = players.First();
                }
            }
        }

        public bool IsHost(string roomId, string playerId)
        {
            return _rooms.TryGetValue(roomId, out var room) && room.HostId == playerId;
        }

        public void RemoveRoom(string roomId)
        {
            _rooms.TryRemove(roomId, out _);
            _roomPlayers.TryRemove(roomId, out _);
        }

        public IEnumerable<string> GetPlayerIdsInRoom(string roomId)
        {
            if (_roomPlayers.TryGetValue(roomId, out var players))
            {
                return players;
            }
            return Enumerable.Empty<string>();
        }
    }
}
