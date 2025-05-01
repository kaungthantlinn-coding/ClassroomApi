using Classroom.Models;
using Classroom.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Repositories.Implementation;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ClassroomContext _context;

    public RefreshTokenRepository(ClassroomContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await SaveChangesAsync();
        return refreshToken;
    }

    public async Task<RefreshToken> UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await SaveChangesAsync();
        return refreshToken;
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}