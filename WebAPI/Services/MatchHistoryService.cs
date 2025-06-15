using Microsoft.Extensions.Logging;
using OmokServer.Models;
using OmokServer.Repositories;
using System;
using System.Threading.Tasks;

namespace OmokServer.Services
{
    public class MatchHistoryService : IMatchHistoryService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ILogger<MatchHistoryService> _logger;

        public MatchHistoryService(IMatchRepository matchRepository, ILogger<MatchHistoryService> logger)
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
        public async Task<IEnumerable<Match>?> GetUserMatchHistoryAsync(int requesterId, int targetUserId)
        {
            if (requesterId != targetUserId)
            {
                _logger.LogWarning("권한 없는 전적 조회 시도. Requester: {RequesterId}, Target: {TargetId}", requesterId, targetUserId);
                return null;
            }

            _logger.LogInformation("전적 조회 서비스 호출. UserId: {UserId}", targetUserId);
            return await _matchRepository.GetMatchesByUserIdAsync(targetUserId);
        }
    }
}