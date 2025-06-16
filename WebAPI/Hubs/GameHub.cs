using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OmokServer.Models.Game;
using OmokServer.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ZLogger;

namespace OmokServer.Hubs
{
    /// <summary>
    /// 실시간 게임 통신을 위한 SignalR 허브
    /// </summary>
    public class GameHub : Hub
    {
        private readonly UserConnectionManager _userConnectionManager;
        private readonly GameRoomManager _gameRoomManager;
        private readonly IMatchHistoryService _matchHistoryService;
        private readonly ILogger<GameHub> _logger;

        public GameHub(UserConnectionManager userConnectionManager,
                       GameRoomManager gameRoomManager,
                       IMatchHistoryService matchHistoryService,
                       ILogger<GameHub> logger)
        {
            _userConnectionManager = userConnectionManager;
            _gameRoomManager = gameRoomManager;
            _matchHistoryService = matchHistoryService;
            _logger = logger;
        }

        /// <summary>
        /// 사용자를 연결 관리자에 등록합니다.
        /// </summary>
        /// <param name="userId">등록할 사용자 ID</param>
        public void Register(int userId)
        {
            _userConnectionManager.AddConnection(userId, Context.ConnectionId);
            _logger.ZLogInformation($"유저 등록됨. UserId: {userId}, ConnectionId: {Context.ConnectionId}");
        }

        /// <summary>
        /// 사용자 연결이 해제될 때 호출됩니다.
        /// </summary>
        /// <param name="exception">연결 해제 예외 (있는 경우)</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.ZLogInformation($"유저 접속 해제. ConnectionId: {Context.ConnectionId}");
            _userConnectionManager.RemoveConnectionByConnectionId(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 게임 보드에 돌을 놓습니다.
        /// </summary>
        /// <param name="roomId">게임방 ID</param>
        /// <param name="x">X 좌표</param>
        /// <param name="y">Y 좌표</param>
        [Authorize]
        public async Task PlaceStone(string roomId, int x, int y)
        {
            var userIdString = Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null) return;
            var userId = int.Parse(userIdString);

            var room = _gameRoomManager.GetRoom(roomId);
            if (room == null)
            {
                _logger.LogWarning("PlaceStone: 존재하지 않는 방입니다. RoomId: {RoomId}", roomId);
                return;
            }

            bool isSuccess = room.PlaceStone(userId, x, y);

            if (!isSuccess)
            {
                _logger.LogWarning("PlaceStone: 돌을 놓는 데 실패했습니다. RoomId: {RoomId}, UserId: {UserId}", roomId, userId);
                return;
            }

            await Clients.Group(roomId).SendAsync("StonePlaced", userId, x, y);
            _logger.ZLogInformation($"StonePlaced 전송. RoomId: {roomId}, UserId: {userId}");

            if (room.Status == GameStatus.Finished)
            {
                var winner = (room.Player1.UserId == userId) ? room.Player1 : room.Player2;
                var loser = (room.Player1.UserId != userId) ? room.Player1 : room.Player2;

                _logger.ZLogInformation($"게임 종료! RoomId: {roomId}, Winner: {winner.Username}");

                await _matchHistoryService.CreateMatchAsync(winner.UserId, loser.UserId);

                await Clients.Group(roomId).SendAsync("GameOver", winner.Username);

                _gameRoomManager.RemoveRoom(roomId);
            }
        }
    }
}