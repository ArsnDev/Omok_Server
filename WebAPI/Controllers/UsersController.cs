using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;
        private readonly IConfiguration _configuration;
        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _logger = logger;
            _configuration = configuration;
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

            // 1. 사용자 이름으로 유저 정보 찾기 (by Repository)
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "아이디 또는 비밀번호가 올바르지 않습니다." });
            }

            // 2. BCrypt를 사용하여 비밀번호 검증
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "아이디 또는 비밀번호가 올바르지 않습니다." });
            }

            // 3. 비밀번호 검증 성공 시, JWT 생성
            var token = GenerateJwtToken(user);

            _logger.LogInformation("[로그인] - 성공, Username: {Username}", request.Username);
            return Ok(new { token });
        }

        // JWT를 생성하는 별도 메서드
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
