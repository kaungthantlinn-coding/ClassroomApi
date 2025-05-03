using Classroom.Models;

namespace Classroom.Repositories.Interface;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task<bool> EmailExistsAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> ChangePasswordAsync(int userId, string newPasswordHash);
    Task<bool> VerifyPasswordAsync(int userId, string password);
    Task<List<User>> GetAllUsersAsync();
    Task SaveChangesAsync();
}