using System;
using System.Security.Cryptography;
using System.Text;

namespace KindergartenApp.Application.Utilities;

public static class PasswordHasher
{
    // Versioned format so we can change parameters later without breaking existing users:
    // PBKDF2$<iterations>$<salt_b64>$<subkey_b64>
    private const string Prefix = "PBKDF2$";
    private const int SaltSizeBytes = 16;
    private const int SubkeySizeBytes = 32;
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);

        var subkey = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: SubkeySizeBytes);

        return $"{Prefix}{Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(subkey)}";
    }

    public static bool Verify(string password, string hash)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));
        if (string.IsNullOrWhiteSpace(hash))
            return false;

        // Backward compatibility: legacy hashes were SHA256(password) base64.
        // Keep this so existing seeded DBs still allow login; new hashes will use PBKDF2.
        if (!hash.StartsWith(Prefix, StringComparison.Ordinal))
        {
            var legacy = LegacySha256Hash(password);
            return FixedTimeEquals(legacy, hash);
        }

        // Expected: PBKDF2$iter$salt$subkey
        var parts = hash.Split('$', StringSplitOptions.None);
        if (parts.Length != 4 || !string.Equals(parts[0], "PBKDF2", StringComparison.Ordinal))
            return false;

        if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
            return false;

        byte[] salt;
        byte[] expectedSubkey;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expectedSubkey = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualSubkey = Rfc2898DeriveBytes.Pbkdf2(
            password: Encoding.UTF8.GetBytes(password),
            salt: salt,
            iterations: iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: expectedSubkey.Length);

        return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
    }

    private static string LegacySha256Hash(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        // Compare as bytes to avoid timing differences; treat null/empty as non-match.
        if (a is null || b is null)
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a),
            Encoding.UTF8.GetBytes(b));
    }
}
