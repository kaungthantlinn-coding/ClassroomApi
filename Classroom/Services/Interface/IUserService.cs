using Classroom.Dtos;
using Classroom.Models;

namespace Classroom.Services.Interface;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
    Task<object?> GetUserClassDataAsync(int userId, int courseId);
    Task<bool> SaveUserClassDataAsync(int userId, int courseId, UserClassDataDto userClassDataDto);
}