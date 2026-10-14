using System;
using System.Security.Cryptography;
using System.Text;

namespace WebHost.Services
{
    /// <summary>
    /// 基于 PBKDF2 (SHA-256) 的密码哈希工具，不依赖第三方库。
    /// 存储格式为 "Base64(salt):Base64(hash)"。
    /// </summary>
    public static class PasswordHasher
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored) || !stored.Contains(':'))
            {
                return false;
            }

            var parts = stored.Split(':', 2);
            if (!TryFromBase64(parts[0], out var salt) || !TryFromBase64(parts[1], out var expected))
            {
                return false;
            }

            var actual = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }

        private static bool TryFromBase64(string s, out byte[] bytes)
        {
            try
            {
                bytes = Convert.FromBase64String(s);
                return true;
            }
            catch
            {
                bytes = Array.Empty<byte>();
                return false;
            }
        }
    }
}
