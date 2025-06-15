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
        public record CreateMatchRequest(int WinnerId, int LoserId);
        [HttpPost]
        public async Task<IActionResult> CreateMatch([FromBody] CreateMatchRequest request)
        {
            _logger.LogInformation("경기 결과 저장 요청 수신. Winner: {WinnerId}, Loser: {LoserId}", request.WinnerId, request.LoserId);
            await _gameService.CreateMatchAsync(request.WinnerId, request.LoserId);
            return Ok(new { message = "경기 결과가 성공적으로 저장되었습니다." });
        }
        [HttpGet("history/{userId}")] // GET /api/matches/history/{userId}
        public async Task<IActionResult> GetUserMatchHistory(int userId)
        {
            var requesterIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (requesterIdString == null)
            {
                return Unauthorized();
            }
            var requesterId = int.Parse(requesterIdString);
            _logger.LogInformation("전적 조회 요청 수신. Requester: {RequesterId}, Target: {TargetId}", requesterId, userId);
            var matchHistory = await _gameService.GetUserMatchHistoryAsync(requesterId, userId);
            if (matchHistory == null)
            {
                return Forbid();
            }

            return Ok(matchHistory);
        }
    }
}