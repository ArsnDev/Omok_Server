using OmokServer.Models.Game;
using System.Collections.Concurrent;

namespace OmokServer.Services
{
    /// <summary>
    /// 게임방 관리 서비스
    /// </summary>
    public class GameRoomManager
    {
        private readonly ConcurrentDictionary<string, GameRoom> _rooms = new ConcurrentDictionary<string, GameRoom>();

        /// <summary>
        /// 새로운 게임방을 생성합니다.
        /// </summary>
        /// <param name="player1">플레이어 1 정보</param>
        /// <param name="player2">플레이어 2 정보</param>
        /// <returns>생성된 게임방</returns>
        public GameRoom CreateRoom(Player player1, Player player2)
        {
            var newRoom = new GameRoom(player1, player2);
            _rooms.TryAdd(newRoom.RoomId, newRoom);
            return newRoom;
        }

        /// <summary>
        /// 특정 ID의 게임방을 조회합니다.
        /// </summary>
        /// <param name="roomId">게임방 ID</param>
        /// <returns>게임방 정보 (있는 경우)</returns>
        public GameRoom? GetRoom(string roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }

        /// <summary>
        /// 게임방을 제거합니다.
        /// </summary>
        /// <param name="roomId">제거할 게임방 ID</param>
        public void RemoveRoom(string roomId)
        {
            _rooms.TryRemove(roomId, out _);
        }
    }
}
