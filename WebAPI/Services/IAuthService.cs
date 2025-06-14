using OmokServer.Models.DTOs;
using System.Threading.Tasks;

namespace OmokServer.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDto request);
        Task<TokenResponseDto?> LoginAsync(LoginRequestDto request);
    }
}