using WebAPI.Models;
using SqlKata.Execution;
using System.Threading.Tasks;

namespace WebAPI.Repositories
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
            return result as User;
        }
    }
}
