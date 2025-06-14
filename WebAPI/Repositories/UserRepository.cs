using OmokServer.Models;
using SqlKata.Execution;
using System.Threading.Tasks;

namespace OmokServer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly QueryFactory _db;
        public UserRepository(QueryFactory db)
        {
            _db = db;
        }
        public async Task AddAsync(User user)
        {
            await _db.Query("Users").InsertAsync(new
            {
                user.Username,
                user.PasswordHash
            });
        }
        public async Task<User?> GetByUsernameAsync(string username)
        {
            var result = await _db.Query("Users").Where("Username", username).FirstOrDefaultAsync<User>();
            return result;
        }
        public async Task<User?> GetUserByIdAsync(int playerId)
        {
            var result = await _db.Query("Users").Where("UserId", playerId).FirstOrDefaultAsync<User>();
            return result;
        }
    }
}
