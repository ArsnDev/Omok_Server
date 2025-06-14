using Microsoft.AspNetCore.SignalR;
using OmokServer.Services;
using System;
using System.Threading.Tasks;

namespace OmokServer.Hubs
{
    public class GameHub : Hub
    {
        private readonly UserConnectionManager _userConnectionManager;
        private readonly ILogger<GameHub> _logger;
        public GameHub(UserConnectionManager userConnectionManager, ILogger<GameHub> logger)
        {
            _userConnectionManager = userConnectionManager;
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
    }
}
