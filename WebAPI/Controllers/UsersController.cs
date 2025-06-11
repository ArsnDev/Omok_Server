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
using WebAPI.Models;
using WebAPI.Repositories;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<UsersController> _logger;
        
        public UsersController(IUserRepository userRepository, IAuthService authService, ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _authService = authService;
            _logger = logger;
        }

        public record RegisterRequest(string Username, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("[회원가입] - Register, Username: {Username}", request.Username);

            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                _logger.LogWarning("[회원가입] - 이미 존재하는 아이디 Username : {Username}", request.Username);
                return BadRequest(new { message = "이미 존재하는 아이디입니다." });
            }
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash
            };

            await _userRepository.AddAsync(newUser);

            _logger.LogInformation("[회원가입] - 성공, Username : {Username}", request.Username);
            return Ok(new { message = "회원가입이 성공적으로 완료되었습니다." });
        }
        public record LoginRequest(string Username, string Password);
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("[로그인] - 요청 시작, Username: {Username}", request.Username);

            var token = await _authService.LoginAsync(request.Username, request.Password);

            if (token == null)
            {
                return Unauthorized(new { message = "아이디 또는 비밀번호가 올바르지 않습니다." });
            }

            return Ok(new { token });
        }
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMyInfo()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (userId == null || username == null)
            {
                return Unauthorized();
            }

            _logger.LogInformation("[내 정보] - {Username}({UserId}) 님이 정보 조회", username, userId);

            return Ok(new { userId = int.Parse(userId), username });
        }
    }
}