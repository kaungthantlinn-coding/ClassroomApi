using Classroom.Dtos;
using Classroom.Models;

namespace Classroom.Services.Interface;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
}