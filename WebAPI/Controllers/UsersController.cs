using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Repositories;
using System.Threading.Tasks;
using BCrypt.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
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
    }
}
