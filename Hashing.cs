using System.Security.Cryptography;

namespace DiscordDrive;

internal static class Hashing
{
    public static string SHA256CheckSum(Stream stream)
    {
        using (SHA256 SHA256 = SHA256Managed.Create())
        {
            string result = "";
            foreach (var hash in SHA256.ComputeHash(stream))
            {
                result += hash.ToString("x2");
            }

            return result;
        }
    }

    public static string SHA256CheckSum(string filePath)
    {
        using (SHA256 SHA256 = SHA256Managed.Create())
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                string result = "";
                foreach (var hash in SHA256.ComputeHash(fileStream))
                {
                    result += hash.ToString("x2");
                }
                return result;
            }
        }
    }
}
