using WebAPI.Models;
using System.Threading.Tasks;

namespace WebAPI.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);

        Task AddAsync(User user);
    }
}
