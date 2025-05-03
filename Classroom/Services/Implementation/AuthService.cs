using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Classroom.Dtos;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Classroom.Services.Implementation;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user already exists
        var userExists = await _userRepository.EmailExistsAsync(registerDto.Email);
        if (userExists)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User with this email already exists"
            };
        }

        // Validate that passwords match (this is also done by the validator, but double-checking here)
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Passwords do not match"
            };
        }

        // Hash password
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Create new user
        var newUser = new User
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            Password = passwordHash,
            Avatar = registerDto.Avatar,
            Role = registerDto.Role,
            UserGuid = Guid.NewGuid()
        };

        await _userRepository.CreateAsync(newUser);

        // Generate tokens
        var tokens = await GenerateTokensAsync(newUser);

        return new AuthResponseDto
        {
            Success = true,
            Message = "User registered successfully",
            Token = tokens.Token,
            RefreshToken = tokens.RefreshToken,
            Expiration = tokens.Expiration,
            User = new UserDto
            {
                UserId = newUser.UserId,
                UserGuid = newUser.UserGuid,
                Name = newUser.Name,
                Email = newUser.Email,
                Avatar = newUser.Avatar,
                Role = newUser.Role
            }
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // Verify password using repository method
        bool isPasswordValid = await _userRepository.VerifyPasswordAsync(user.UserId, loginDto.Password);
        if (!isPasswordValid)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }

        // Generate tokens
        var tokens = await GenerateTokensAsync(user);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Login successful",
            Token = tokens.Token,
            RefreshToken = tokens.RefreshToken,
            Expiration = tokens.Expiration,
            User = new UserDto
            {
                UserId = user.UserId,
                UserGuid = user.UserGuid,
                Name = user.Name,
                Email = user.Email,
                Avatar = user.Avatar,
                Role = user.Role
            }
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        var validatedToken = GetPrincipalFromExpiredToken(refreshTokenDto.Token);
        if (validatedToken == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid token"
            };
        }

        var userId = int.Parse(validatedToken.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshTokenDto.RefreshToken);

        if (storedRefreshToken == null ||
            storedRefreshToken.UserId != userId ||
            storedRefreshToken.IsUsed ||
            storedRefreshToken.IsRevoked ||
            storedRefreshToken.ExpiryDate < DateTime.UtcNow)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid refresh token"
            };
        }

        // Mark the current refresh token as used
        storedRefreshToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(storedRefreshToken);

        // Get user
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User not found"
            };
        }

        // Generate new tokens
        var tokens = await GenerateTokensAsync(user);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Token refreshed successfully",
            Token = tokens.Token,
            RefreshToken = tokens.RefreshToken,
            Expiration = tokens.Expiration,
            User = new UserDto
            {
                UserId = user.UserId,
                UserGuid = user.UserGuid,
                Name = user.Name,
                Email = user.Email,
                Avatar = user.Avatar,
                Role = user.Role
            }
        };
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return new UserDto();
        }

        return new UserDto
        {
            UserId = user.UserId,
            UserGuid = user.UserGuid,
            Name = user.Name,
            Email = user.Email,
            Avatar = user.Avatar,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
    {
        // Check if user exists
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User not found"
            };
        }

        // Verify current password using repository method
        bool isCurrentPasswordValid = await _userRepository.VerifyPasswordAsync(userId, changePasswordDto.CurrentPassword);
        if (!isCurrentPasswordValid)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Current password is incorrect"
            };
        }

        // Verify that new password is different from current password
        if (changePasswordDto.CurrentPassword == changePasswordDto.NewPassword)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "New password must be different from current password"
            };
        }

        // Verify that new password and confirm password match
        if (changePasswordDto.NewPassword != changePasswordDto.ConfirmNewPassword)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "New password and confirm password do not match"
            };
        }

        // Hash new password
        string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);

        // Update user's password using repository method
        bool success = await _userRepository.ChangePasswordAsync(userId, newPasswordHash);
        if (!success)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Failed to change password"
            };
        }

        return new AuthResponseDto
        {
            Success = true,
            Message = "Password changed successfully"
        };
    }

    private async Task<(string Token, string RefreshToken, DateTime Expiration)> GenerateTokensAsync(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            JwtId = token.Id,
            UserId = user.UserId,
            AddedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            IsUsed = false,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);

        return (new JwtSecurityTokenHandler().WriteToken(token), refreshToken.Token, expiry);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false // Allow expired tokens
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}