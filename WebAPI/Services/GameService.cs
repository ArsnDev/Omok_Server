using Microsoft.Extensions.Logging;
using OmokServer.Models;
using OmokServer.Repositories;
using System;
using System.Threading.Tasks;

namespace OmokServer.Services
{
    public class GameService : IGameService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ILogger<GameService> _logger;

        public GameService(IMatchRepository matchRepository, ILogger<GameService> logger)
        {
            _matchRepository = matchRepository;
            _logger = logger;
        }
        public async Task CreateMatchAsync(int winnerId, int loserId)
        {
            var newMatch = new Match
            {
                WinnerId = winnerId,
                LoserId = loserId,
                MatchDate = DateTime.UtcNow,
            };
            await _matchRepository.AddMatchAsync(newMatch);
            _logger.LogInformation("경기 결과 저장 성공. Winner: {WinnerId}, Loser: {LoserId}", winnerId, loserId);
        }
        public async Task<IEnumerable<Match>> GetUserMatchHistoryAsync(int userId)
        {
            _logger.LogInformation("전적 조회 서비스 호출. UserId: {UserId}", userId);
            return await _matchRepository.GetMatchesByUserIdAsync(userId);
        }
    }
}