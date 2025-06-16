using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OmokServer.Models;
using SqlKata.Execution;
using System.Threading.Tasks;


namespace OmokServer.Controllers
{
    /// <summary>
    /// 테스트용 컨트롤러 (더미 데이터 생성 및 조회)
    /// </summary>
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

        /// <summary>
        /// 모든 사용자 목록을 조회합니다.
        /// </summary>
        /// <returns>사용자 목록</returns>
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            // SqlKata를 사용하여 쿼리 생성 및 실행
            var users = await _db.Query("Users").GetAsync<User>();
            return Ok(users);
        }

        /// <summary>
        /// 테스트용 더미 사용자를 생성합니다.
        /// </summary>
        /// <returns>생성된 사용자 정보</returns>
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
