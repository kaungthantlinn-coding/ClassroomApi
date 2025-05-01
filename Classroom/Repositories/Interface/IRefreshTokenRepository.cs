using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task<RefreshToken> UpdateAsync(RefreshToken refreshToken);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId);
    Task SaveChangesAsync();
}