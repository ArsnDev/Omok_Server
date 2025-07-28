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
        private readonly MatchmakingService _matchmakingService;
        private readonly ILogger<GameHub> _logger;

        public GameHub(UserConnectionManager userConnectionManager,
                       GameRoomManager gameRoomManager,
                       IMatchHistoryService matchHistoryService,
                       MatchmakingService matchmakingService,
                       ILogger<GameHub> logger)
        {
            _userConnectionManager = userConnectionManager;
            _gameRoomManager = gameRoomManager;
            _matchHistoryService = matchHistoryService;
            _matchmakingService = matchmakingService;
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
            
            // 연결 관리자에서 사용자 제거하고 UserId 획득
            var userId = _userConnectionManager.GetUserIdByConnectionId(Context.ConnectionId);
            _userConnectionManager.RemoveConnectionByConnectionId(Context.ConnectionId);
            
            // 대기열에서도 사용자 제거
            if (userId.HasValue)
            {
                _matchmakingService.RemoveFromQueue(userId.Value);
                _logger.ZLogInformation($"유저 {userId.Value}를 대기열에서 제거했습니다.");
            }
            
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
            if (userIdString == null) 
            {
                await Clients.Caller.SendAsync("ActionResult", new { success = false, message = "인증 실패" });
                return;
            }
            var userId = int.Parse(userIdString);

            var room = _gameRoomManager.GetRoom(roomId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("ActionResult", new { success = false, message = "존재하지 않는 게임방입니다." });
                return;
            }

            _logger.ZLogInformation($"PlaceStone 요청: RoomId={roomId}, UserId={userId}, X={x}, Y={y}");

            bool isSuccess = room.PlaceStone(userId, x, y);

            if (!isSuccess)
            {
                await Clients.Caller.SendAsync("ActionResult", new { success = false, message = "돌을 놓을 수 없습니다." });
                return;
            }

            // 성공 시 모든 플레이어에게 전체 게임 상태 전송
            await BroadcastGameState(room);

            // 게임 종료 체크
            if (room.Status == GameStatus.Finished)
            {
                var winner = (room.Player1.UserId == userId) ? room.Player1 : room.Player2;
                var loser = (room.Player1.UserId != userId) ? room.Player1 : room.Player2;

                _logger.ZLogInformation($"게임 종료! RoomId: {roomId}, Winner: {winner.Username}");
                
                // 게임 종료 이벤트 전송
                await Clients.Group(roomId).SendAsync("GameOver", winner.Username);
                
                // 매치 히스토리 저장 및 게임방 제거
                await _matchHistoryService.CreateMatchAsync(winner.UserId, loser.UserId);
                _gameRoomManager.RemoveRoom(roomId);
            }
        }

        /// <summary>
        /// 게임방의 전체 상태를 모든 플레이어에게 전송합니다.
        /// </summary>
        private async Task BroadcastGameState(GameRoom room)
        {
            var gameState = new
            {
                roomId = room.RoomId,
                board = room.GetBoard(),
                player1 = new { userId = room.Player1.UserId, username = room.Player1.Username },
                player2 = new { userId = room.Player2.UserId, username = room.Player2.Username },
                currentTurnPlayerId = room.TurnOwnerPlayerId,
                gameStatus = room.Status.ToString()
            };

            await Clients.Group(room.RoomId).SendAsync("GameStateUpdate", gameState);
            _logger.ZLogInformation($"GameState 전송: RoomId={room.RoomId}, CurrentTurn={room.TurnOwnerPlayerId}");
        }
    }
}