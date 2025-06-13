using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using OmokServer.Models;
using OmokServer.DTOs;
using OmokServer.Services;

namespace OmokServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<UsersController> _logger;
        
        public UsersController(IAuthService authService, ILogger<UsersController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            _logger.LogInformation("[컨트롤러] 회원가입 요청 수신: {Username}", request.Username);
            var isSuccess = await _authService.RegisterAsync(request);

            if (!isSuccess)
            {
                return BadRequest(new { message = "이미 존재하는 아이디입니다." });
            }
            return Ok(new { message = "회원가입이 성공적으로 완료되었습니다." });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            _logger.LogInformation("[컨트롤러] 로그인 요청 수신: {Username}", request.Username);
            var tokenResponse = await _authService.LoginAsync(request);

            if (tokenResponse == null)
            {
                return Unauthorized(new { message = "아이디 또는 비밀번호가 올바르지 않습니다." });
            }
            return Ok(tokenResponse);
        }
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMyInfo()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (userId == null || username == null) return Unauthorized();

            var userDto = new UserDto(int.Parse(userId), username);
            return Ok(userDto);
        }
    }
}