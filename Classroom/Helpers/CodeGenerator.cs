using System.Security.Cryptography;

namespace Classroom.Helpers;

public static class CodeGenerator
{
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string GenerateCode(int length = 6)
    {
        // Create a byte array for the random values
        byte[] randomBytes = new byte[length];

        // Fill the array with random values using a cryptographically secure RNG
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        // Convert random bytes to characters from our allowed set
        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            // Ensure even distribution by using modulo
            result[i] = Characters[randomBytes[i] % Characters.Length];
        }

        return new string(result);
    }

    public static string GenerateUniqueCode(IEnumerable<string> existingCodes, int length = 6)
    {
        string newCode;
        bool exists;

        // Try until we find a unique code
        do
        {
            newCode = GenerateCode(length);
            exists = existingCodes.Contains(newCode);
        } while (exists);

        return newCode;
    }
}
