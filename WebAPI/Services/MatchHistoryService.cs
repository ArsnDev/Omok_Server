using Microsoft.Extensions.Logging;
using OmokServer.Models;
using OmokServer.Repositories;
using System;
using System.Threading.Tasks;
using ZLogger;

namespace OmokServer.Services
{
    /// <summary>
    /// 경기 결과 저장 및 전적 조회 서비스
    /// </summary>
    public class MatchHistoryService : IMatchHistoryService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly ILogger<MatchHistoryService> _logger;

        public MatchHistoryService(IMatchRepository matchRepository, ILogger<MatchHistoryService> logger)
        {
            _matchRepository = matchRepository;
            _logger = logger;
        }

        /// <summary>
        /// 새로운 경기 결과를 저장합니다.
        /// </summary>
        /// <param name="winnerId">승자 ID</param>
        /// <param name="loserId">패자 ID</param>
        public async Task CreateMatchAsync(int winnerId, int loserId)
        {
            var newMatch = new Match
            {
                WinnerId = winnerId,
                LoserId = loserId,
                MatchDate = DateTime.UtcNow,
            };
            await _matchRepository.AddMatchAsync(newMatch);
            _logger.ZLogInformation($"경기 결과 저장 성공. Winner: {winnerId}, Loser: {loserId}");
        }

        /// <summary>
        /// 특정 사용자의 경기 전적을 조회합니다.
        /// </summary>
        /// <param name="requesterId">요청자 ID</param>
        /// <param name="targetUserId">조회할 사용자 ID</param>
        /// <returns>경기 전적 목록</returns>
        public async Task<IEnumerable<Match>?> GetUserMatchHistoryAsync(int requesterId, int targetUserId)
        {
            if (requesterId != targetUserId)
            {
                _logger.LogWarning("권한 없는 전적 조회 시도. Requester: {RequesterId}, Target: {TargetId}", requesterId, targetUserId);
                return null;
            }

            _logger.ZLogInformation($"전적 조회 서비스 호출. UserId: {targetUserId}");
            return await _matchRepository.GetMatchesByUserIdAsync(targetUserId);
        }
    }
}