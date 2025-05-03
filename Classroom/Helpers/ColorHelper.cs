using System.Security.Cryptography;

namespace Classroom.Helpers;

public static class ColorHelper
{
    // Predefined color palette for course backgrounds
    private static readonly string[] BackgroundColors = new[]
    {
        "#4285F4", // Google Blue
        "#34A853", // Google Green
        "#EA4335", // Google Red
        "#FBBC05", // Google Yellow
        "#FF6D01", // Orange
        "#46BDC6", // Teal
        "#9C27B0", // Purple
        "#607D8B", // Blue Grey
        "#795548", // Brown
        "#009688", // Material Teal
        "#673AB7", // Deep Purple
        "#3F51B5", // Indigo
        "#2196F3", // Light Blue
        "#03A9F4", // Cyan
        "#00BCD4", // Aqua
        "#8BC34A", // Light Green
        "#CDDC39", // Lime
        "#FFC107", // Amber
        "#FF9800", // Orange
        "#FF5722"  // Deep Orange
    };

    // Generate a random color from our palette
    public static string GetRandomBackgroundColor()
    {
        int randomIndex = RandomNumberGenerator.GetInt32(0, BackgroundColors.Length);
        return BackgroundColors[randomIndex];
    }

    // Determine if text should be white or black based on background color brightness
    public static string GetTextColorForBackground(string backgroundColor)
    {
        // Default to black if the color is invalid
        if (string.IsNullOrEmpty(backgroundColor) || !backgroundColor.StartsWith("#"))
            return "#000000";

        // Remove the # character
        string colorHex = backgroundColor.TrimStart('#');

        // Handle both 3-character and 6-character hex codes
        if (colorHex.Length == 3)
        {
            colorHex = string.Concat(colorHex.Select(c => new string(c, 2)));
        }

        if (colorHex.Length != 6)
            return "#000000";

        // Convert hex to RGB
        int r = Convert.ToInt32(colorHex.Substring(0, 2), 16);
        int g = Convert.ToInt32(colorHex.Substring(2, 2), 16);
        int b = Convert.ToInt32(colorHex.Substring(4, 2), 16);

        // Calculate brightness using the formula: (0.299*R + 0.587*G + 0.114*B)
        double brightness = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

        // Use white text for dark backgrounds, black text for light backgrounds
        return brightness > 0.5 ? "#000000" : "#FFFFFF";
    }
}
