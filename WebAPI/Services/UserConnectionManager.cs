using System.Collections.Concurrent;

namespace OmokServer.Services
{
    public class UserConnectionManager
    {
        private readonly ConcurrentDictionary<int, string> _userConnections = new ConcurrentDictionary<int, string>();
        public void AddConnection(int userId, string connectionId)
        {
            _userConnections[userId] = connectionId;
        }
        public void RemoveConnectionByConnectionId(string connectionId)
        {
            var item = _userConnections.FirstOrDefault(p => p.Value == connectionId);
            if (item.Key != 0)
            {
                _userConnections.TryRemove(item.Key, out _);
            }
        }
        public string? GetConnectionId(int userId)
        {
            _userConnections.TryGetValue(userId, out var connectionId);
            return connectionId;
        }
    }
}
