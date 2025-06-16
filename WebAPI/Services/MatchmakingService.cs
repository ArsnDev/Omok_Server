using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OmokServer.Hubs;
using OmokServer.Models;
using OmokServer.Repositories;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ZLogger;

namespace OmokServer.Services
{
    /// <summary>
    /// 매칭 시스템 서비스
    /// </summary>
    public class MatchmakingService
    {
        private readonly ConcurrentQueue<int> _waitingQueue = new ConcurrentQueue<int>();
        private readonly object _matchmakingLock = new object();
        private readonly ILogger<MatchmakingService> _logger;
        private readonly GameRoomManager _gameRoomManager;
        private readonly IHubContext<GameHub> _hubContext;
        private readonly UserConnectionManager _userConnectionManager;
        private readonly IServiceProvider _serviceProvider;

        public MatchmakingService(ILogger<MatchmakingService> logger,
                                  GameRoomManager gameRoomManager,
                                  IHubContext<GameHub> hubContext,
                                  UserConnectionManager userConnectionManager,
                                  IServiceProvider serviceProvider)
        {
            _logger = logger;
            _gameRoomManager = gameRoomManager;
            _hubContext = hubContext;
            _userConnectionManager = userConnectionManager;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 매칭 대기열에 사용자를 추가합니다.
        /// </summary>
        /// <param name="userId">추가할 사용자 ID</param>
        public void AddToQueue(int userId)
        {
            if (!_waitingQueue.Contains(userId))
            {
                _waitingQueue.Enqueue(userId);
                _logger.ZLogInformation($"유저 {userId}가 대기열에 추가되었습니다. 현재 대기인원: {_waitingQueue.Count}");
            }
        }

        /// <summary>
        /// 매칭 가능한 플레이어 쌍을 찾습니다.
        /// </summary>
        /// <returns>매칭된 플레이어 쌍 (있는 경우)</returns>
        public (int Player1, int Player2)? TryGetMatchedPair()
        {
            lock (_matchmakingLock)
            {
                if (_waitingQueue.Count >= 2)
                {
                    if (_waitingQueue.TryDequeue(out var player1Id) && _waitingQueue.TryDequeue(out var player2Id))
                    {
                        return (player1Id, player2Id);
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 매칭된 플레이어들에 대해 게임방을 생성하고 처리합니다.
        /// </summary>
        /// <param name="player1Id">플레이어 1 ID</param>
        /// <param name="player2Id">플레이어 2 ID</param>
        public async Task ProcessMatchAsync(int player1Id, int player2Id)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var player1 = await userRepository.GetUserByIdAsync(player1Id);
                var player2 = await userRepository.GetUserByIdAsync(player2Id);

                if (player1 == null || player2 == null)
                {
                    _logger.ZLogError($"매칭되었으나 DB에서 유저 정보를 찾을 수 없습니다. P1: {player1Id}, P2: {player2Id}");
                    if (player1 != null) AddToQueue(player1Id);
                    if (player2 != null) AddToQueue(player2Id);
                    return;
                }
                var newRoom = _gameRoomManager.CreateRoom(new Models.Game.Player(player1.UserId, player1.Username), new Models.Game.Player(player2.UserId, player2.Username));
                var player1ConnectionId = _userConnectionManager.GetConnectionId(player1Id);
                var player2ConnectionId = _userConnectionManager.GetConnectionId(player2Id);
                if (player1ConnectionId != null && player2ConnectionId != null)
                {
                    await _hubContext.Groups.AddToGroupAsync(player1ConnectionId, newRoom.RoomId);
                    await _hubContext.Groups.AddToGroupAsync(player2ConnectionId, newRoom.RoomId);

                    await _hubContext.Clients.Client(player1ConnectionId).SendAsync("MatchFound", new { RoomId = newRoom.RoomId, OpponentName = player2.Username });
                    await _hubContext.Clients.Client(player2ConnectionId).SendAsync("MatchFound", new { RoomId = newRoom.RoomId, OpponentName = player1.Username });

                    _logger.ZLogInformation($"매칭 성공! RoomId: {newRoom.RoomId}, P1: {player1Id}, P2: {player2Id}");
                }
                else
                {
                    _gameRoomManager.RemoveRoom(newRoom.RoomId);
                    if (player1ConnectionId != null) _waitingQueue.Enqueue(player1Id);
                    if (player2ConnectionId != null) _waitingQueue.Enqueue(player2Id);
                    _logger.ZLogWarning($"매칭되었으나 상대방이 오프라인 상태입니다. 매칭을 취소합니다.");
                }
            }
        }
    }
}