using WebAPI.Models;
using System.Collections.Generic;

namespace WebAPI.Services
{
    public interface IGameService
    {
        Task CreateMatchAsync(int winnerId, int loserId);
        Task<IEnumerable<Match>> GetUserMatchHistoryAsync(int userId);
    }
}