using Classroom.Dtos;

namespace Classroom.Services.Interface;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<UserDto> GetCurrentUserAsync(int userId);
}