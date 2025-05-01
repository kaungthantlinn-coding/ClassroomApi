namespace Classroom.Models;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
    public int RefreshTokenExpiryDays { get; set; }
    public int ExpiryInMinutes { get; set; }
    public int RefreshTokenExpiryInDays { get; set; }
}