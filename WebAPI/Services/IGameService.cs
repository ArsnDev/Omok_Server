using OmokServer.Models;
using System.Collections.Generic;

namespace OmokServer.Services
{
    public interface IGameService
    {
        Task CreateMatchAsync(int winnerId, int loserId);
        Task<IEnumerable<Match>?> GetUserMatchHistoryAsync(int requesterId, int targetUserId);
    }
}