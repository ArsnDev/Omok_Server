using OmokServer.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OmokServer.Repositories
{
    public interface IMatchRepository
    {
        Task AddMatchAsync(Match match);
        Task<IEnumerable<Match>> GetMatchesByUserIdAsync(int userId);
    }
}