using System.Security.Cryptography;
using System.Text;

namespace IoTmate.Common.Security;

public static class CryptoUtils
{
    public static string GenerateSecureToken(int byteLength = 32)
    {
        var randomBytes = new byte[byteLength];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToHexStringLower(randomBytes);
    }

    public static byte[] HashTokenToBytes(string token)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(token));
    }

    public static string HashTokenToHex(string token)
    {
        return Convert.ToHexStringLower(HashTokenToBytes(token));
    }

    public static bool FixedTimeEqualsHex(string storedHex, string incomingToken)
    {
        if (string.IsNullOrEmpty(storedHex) || string.IsNullOrEmpty(incomingToken))
            return false;

        try
        {
            byte[] storedBytes = Convert.FromHexString(storedHex);
            byte[] incomingBytes = HashTokenToBytes(incomingToken);

            return CryptographicOperations.FixedTimeEquals(storedBytes, incomingBytes);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}