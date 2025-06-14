using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmokServer.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OmokServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MatchmakingController : ControllerBase
    {
        private readonly MatchmakingService _matchmakingService;
        private readonly ILogger<MatchmakingController> _logger;
        public MatchmakingController(MatchmakingService matchmakingService, ILogger<MatchmakingController> logger)
        {
            _matchmakingService = matchmakingService;
            _logger = logger;
        }
        [HttpPost("queue")]
        public async Task<IActionResult> Enqueue()
        {
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdString == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdString);

            _logger.LogInformation("[매칭 컨트롤러] - 요청 수신. UserId: {UserId}", userId);

            _matchmakingService.AddToQueue(userId);

            var matchedPair = _matchmakingService.TryGetMatchedPair();
            if (matchedPair.HasValue)
            {
                await _matchmakingService.ProcessMatchAsync(matchedPair.Value.Player1, matchedPair.Value.Player2);
            }
            return Ok(new { message = "대기열에 등록되었거나 매칭 처리 중입니다." });
        }
    }
}
