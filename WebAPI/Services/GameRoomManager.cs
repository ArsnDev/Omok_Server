using OmokServer.Models.Game;
using System.Collections.Concurrent;

namespace OmokServer.Services
{
    public class GameRoomManager
    {
        private readonly ConcurrentDictionary<string, GameRoom> _rooms = new ConcurrentDictionary<string, GameRoom>();
        public GameRoom CreateRoom(Player player1, Player player2)
        {
            var newRoom = new GameRoom(player1, player2);
            _rooms.TryAdd(newRoom.RoomId, newRoom);
            return newRoom;
        }
        public GameRoom? GetRoom(string roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }
        public void RemoveRoom(string roomId)
        {
            _rooms.TryRemove(roomId, out _);
        }
    }
}
