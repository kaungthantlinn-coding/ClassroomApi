using Classroom.Models;
using Classroom.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Repositories.Implementation;

public class UserRepository : IUserRepository
{
    private readonly ClassroomContext _context;

    public UserRepository(ClassroomContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await SaveChangesAsync();
        return user;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string newPasswordHash)
    {
        var user = await GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.Password = newPasswordHash;
        _context.Users.Update(user);
        await SaveChangesAsync();
        return true;
    }

    public async Task<bool> VerifyPasswordAsync(int userId, string password)
    {
        var user = await GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, user.Password);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}