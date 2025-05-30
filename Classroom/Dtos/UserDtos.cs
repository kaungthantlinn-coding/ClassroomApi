using System.ComponentModel.DataAnnotations;

namespace Classroom.Dtos;

public class UpdateUserDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    public string? Role { get; set; }
}

public class UserSettingsDto
{
    public bool Success { get; set; } = true;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? EmailNotifications { get; set; }
}