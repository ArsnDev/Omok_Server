using WebAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebAPI.Repositories
{
    public interface IMatchRepository
    {
        Task AddMatchAsync(Match match);
        Task<IEnumerable<Match>> GetMatchesByUserIdAsync(int userId);
    }
}