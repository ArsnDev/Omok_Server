using WebAPI.Models;
using SqlKata.Execution;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace WebAPI.Repositories
{
    public class MatchRepository:IMatchRepository
    {
        private readonly QueryFactory _db;

        public MatchRepository(QueryFactory db)
        {
            _db = db;
        }

        public async Task AddMatchAsync(Match match)
        {
            await _db.Query("Matches").InsertAsync(new
            {
                match.WinnerId,
                match.LoserId,
                match.MatchDate
            });
        }
        public async Task<IEnumerable<Match>> GetMatchesByUserIdAsync(int userId)
        {
            return await _db.Query("Matches")
                            .Where(q => q.Where("WinnerId", userId).OrWhere("LoserId", userId))
                            .OrderByDesc("MatchDate")
                            .GetAsync<Match>();
        }
    }
}