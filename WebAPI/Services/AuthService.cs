using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OmokServer.Models;
using OmokServer.Models.DTOs;
using OmokServer.Repositories;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ZLogger;

namespace OmokServer.Services
{
    /// <summary>
    /// 사용자 인증 서비스
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 새로운 사용자를 등록합니다.
        /// </summary>
        /// <param name="request">회원가입 요청 정보</param>
        /// <returns>등록 성공 여부</returns>
        public async Task<bool> RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser == null)
            {
                _logger.LogWarning("[회원가입] - 이미 존재하는 아이디 Username : {Username}", request.Username);
                return false;
            }
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash
            };
            await _userRepository.AddAsync(newUser);

            _logger.ZLogInformation($"[회원가입] - 성공, Username : {request.Username}");
            return true;
        }

        /// <summary>
        /// 사용자 로그인을 처리하고 JWT 토큰을 생성합니다.
        /// </summary>
        /// <param name="request">로그인 요청 정보</param>
        /// <returns>JWT 토큰이 포함된 응답</returns>
        public async Task<TokenResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("[로그인 서비스] - 인증 실패, Username: {Username}", request.Username);
                return null;
            }

            _logger.ZLogInformation($"[로그인 서비스] - 로그인 성공, Username: {request.Username}");
            var tokenString = GenerateJwtToken(user);

            return new TokenResponseDto(tokenString);
        }

        /// <summary>
        /// JWT 토큰을 생성합니다.
        /// </summary>
        /// <param name="user">토큰을 생성할 사용자 정보</param>
        /// <returns>생성된 JWT 토큰 문자열</returns>
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
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
