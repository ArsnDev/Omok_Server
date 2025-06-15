using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OmokServer.Models.Game;
using OmokServer.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OmokServer.Hubs
{
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

        public void Register(int userId)
        {
            _userConnectionManager.AddConnection(userId, Context.ConnectionId);
            _logger.LogInformation("유저 등록됨. UserId: {UserId}, ConnectionId: {ConnectionId}", userId, Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("유저 접속 해제. ConnectionId: {ConnectionId}", Context.ConnectionId);
            _userConnectionManager.RemoveConnectionByConnectionId(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

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
            _logger.LogInformation("StonePlaced 전송. RoomId: {RoomId}, UserId: {UserId}", roomId, userId);

            if (room.Status == GameStatus.Finished)
            {
                var winner = (room.Player1.UserId == userId) ? room.Player1 : room.Player2;
                var loser = (room.Player1.UserId != userId) ? room.Player1 : room.Player2;

                _logger.LogInformation("게임 종료! RoomId: {RoomId}, Winner: {Winner}", roomId, winner.Username);

                await _matchHistoryService.CreateMatchAsync(winner.UserId, loser.UserId);

                await Clients.Group(roomId).SendAsync("GameOver", winner.Username);

                _gameRoomManager.RemoveRoom(roomId);
            }
        }
    }
}