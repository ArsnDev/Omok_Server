using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using SqlKata.Execution;
using System.Threading.Tasks;
using BCrypt.Net;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly QueryFactory _db;
        private readonly ILogger<UsersController> _logger;

        public UsersController(QueryFactory db, ILogger<UsersController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public record RegisterRequest(string Username, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("[회원가입] - Register, Username: {Username}", request.Username);

            var existingUser = await _db.Query("Users").Where("Username", request.Username).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                _logger.LogWarning("[회원가입] - 이미 존재하는 아이디 Username : {Username}", request.Username);
                return BadRequest(new { message = "이미 존재하는 아이디입니다." });
            }
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            try
            {
                await _db.Query("Users").InsertAsync(new
                {
                    Username = request.Username,
                    PasswordHash = passwordHash
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "[회원가입] - DB 저장 중 예외발생 Username : {Username}", request.Username);
                return StatusCode(500, new { message = "서버 내부 오류 발생" });
            }

            _logger.LogInformation("[회원가입] - 성공, Username : {Username}", request.Username);
            return Ok(new { message = "회원가입이 성공적으로 완료되었습니다." });
        }
    }
}
