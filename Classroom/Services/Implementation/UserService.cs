using Classroom.Dtos;
using Classroom.Models;
using Classroom.Repositories.Interface;
using Classroom.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Services.Implementation;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        // Get all users from database
        // We're using EF Core directly with the context in the repository
        var users = await _userRepository.GetAllUsersAsync();
        return users.Select(u => new UserDto
        {
            UserId = u.UserId,
            UserGuid = u.UserGuid,
            Name = u.Name,
            Email = u.Email,
            Avatar = u.Avatar,
            Role = u.Role
        }).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
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

    public async Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        // Update user properties
        user.Name = updateUserDto.Name;
        user.Email = updateUserDto.Email;
        user.Avatar = updateUserDto.Avatar;

        // Only update role if it's provided and not empty
        if (!string.IsNullOrEmpty(updateUserDto.Role))
        {
            user.Role = updateUserDto.Role;
        }

        await _userRepository.UpdateAsync(user);

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
}