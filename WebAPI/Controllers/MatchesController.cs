using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using OmokServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using OmokServer.Services;
using ZLogger;

namespace OmokServer.Controllers
{
    /// <summary>
    /// 경기 결과 저장 및 전적 조회를 담당하는 컨트롤러
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchHistoryService _gameService;
        private readonly ILogger<MatchesController> _logger;

        public MatchesController(IMatchHistoryService gameService, ILogger<MatchesController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        /// <summary>
        /// 경기 결과를 저장하는 요청 DTO
        /// </summary>
        public record CreateMatchRequest(int WinnerId, int LoserId);

        /// <summary>
        /// 새로운 경기 결과를 저장합니다.
        /// </summary>
        /// <param name="request">승자와 패자의 ID를 포함한 요청 객체</param>
        /// <returns>저장 성공 여부</returns>
        [HttpPost]
        public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request)
        {
            _logger.ZLogInformation($"경기 결과 저장 요청 수신. Winner: {request.WinnerId}, Loser: {request.LoserId}");
            await _gameService.CreateMatchAsync(request.WinnerId, request.LoserId);
            return Ok(new { message = "경기 결과가 성공적으로 저장되었습니다." });
        }

        /// <summary>
        /// 특정 사용자의 경기 전적을 조회합니다.
        /// </summary>
        /// <param name="userId">조회할 사용자의 ID</param>
        /// <returns>사용자의 경기 전적 목록</returns>
        [HttpGet("history/{userId}")] // GET /api/matches/history/{userId}
        public async Task<IActionResult> GetUserMatchHistory(int userId)
        {
            var requesterIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (requesterIdString == null)
            {
                return Unauthorized();
            }
            var requesterId = int.Parse(requesterIdString);
            _logger.ZLogInformation($"전적 조회 요청 수신. Requester: {requesterId}, Target: {userId}");
            var matchHistory = await _gameService.GetUserMatchHistoryAsync(requesterId, userId);
            if (matchHistory == null)
            {
                return Forbid();
            }

            return Ok(matchHistory);
        }
    }
}