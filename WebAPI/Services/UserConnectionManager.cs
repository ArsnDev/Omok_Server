using System.Collections.Concurrent;

namespace OmokServer.Services
{
    /// <summary>
    /// 사용자 연결 관리 서비스
    /// </summary>
    public class UserConnectionManager
    {
        private readonly ConcurrentDictionary<int, string> _userConnections = new ConcurrentDictionary<int, string>();

        /// <summary>
        /// 사용자와 연결 ID를 등록합니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="connectionId">연결 ID</param>
        public void AddConnection(int userId, string connectionId)
        {
            _userConnections[userId] = connectionId;
        }

        /// <summary>
        /// 연결 ID로 사용자 연결을 제거합니다.
        /// </summary>
        /// <param name="connectionId">제거할 연결 ID</param>
        public void RemoveConnectionByConnectionId(string connectionId)
        {
            var item = _userConnections.FirstOrDefault(p => p.Value == connectionId);
            if (item.Key != 0)
            {
                _userConnections.TryRemove(item.Key, out _);
            }
        }

        /// <summary>
        /// 사용자 ID로 연결 ID를 조회합니다.
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>연결 ID (있는 경우)</returns>
        public string? GetConnectionId(int userId)
        {
            _userConnections.TryGetValue(userId, out var connectionId);
            return connectionId;
        }
    }
}
