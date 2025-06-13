using OmokServer.Models;
using System.Threading.Tasks;

namespace OmokServer.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);

        Task AddAsync(User user);
    }
}
