using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmokServer.Models;
using SqlKata.Execution;
using System.Threading.Tasks;


namespace OmokServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly QueryFactory _db;

        // DI를 통해 QueryFactory를 주입받습니다.
        public TestController(QueryFactory db)
        {
            _db = db;
        }

        // GET /api/test/users
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            // SqlKata를 사용하여 쿼리 생성 및 실행
            var users = await _db.Query("Users").GetAsync<User>();
            return Ok(users);
        }

        // GET /api/test/insert-dummy
        [HttpGet("insert-dummy")]
        public async Task<IActionResult> InsertDummyUser()
        {
            // 임시 더미 데이터 삽입
            var affected = await _db.Query("Users").InsertAsync(new
            {
                Username = $"user_{Guid.NewGuid().ToString().Substring(0, 8)}",
                PasswordHash = "dummy_hash",
            });

            return Ok(new { RowsAffected = affected });
        }
    }
}
