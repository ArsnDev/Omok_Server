namespace OmokServer.DTOs
{
        public record RegisterRequestDto(string Username, string Password);
        public record LoginRequestDto(string Username, string Password);
        // Response DTO
        public record TokenResponseDto(string Token);
        public record UserDto(int UserId, string Username);
}