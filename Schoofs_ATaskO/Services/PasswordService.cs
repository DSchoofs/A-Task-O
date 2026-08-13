using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Schoofs_ATaskO.Settings;
using System.Security.Cryptography;

namespace Schoofs_ATaskO.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly SecuritySettings _securitySettings;

        public PasswordService(IOptions<SecuritySettings> securityOptions)
        {
            _securitySettings = securityOptions.Value;
        }

        public string GenerateSalt()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        }

        public string HashPassword(string password, string saltBase64)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Das Passwort darf nicht leer sein.", nameof(password));
            }

            if (string.IsNullOrWhiteSpace(saltBase64))
            {
                throw new ArgumentException("Der Salt darf nicht leer sein.", nameof(saltBase64));
            }

            if (string.IsNullOrWhiteSpace(_securitySettings.PasswordPepper))
            {
                throw new InvalidOperationException("PasswordPepper ist nicht konfiguriert.");
            }

            var saltBytes = Convert.FromBase64String(saltBase64);

            var passwordWithPepper = password + _securitySettings.PasswordPepper;

            var hashBytes = KeyDerivation.Pbkdf2(
                password: passwordWithPepper,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string saltBase64, string expectedHashBase64)
        {
            if (string.IsNullOrWhiteSpace(expectedHashBase64))
            {
                return false;
            }

            var computedHashBase64 = HashPassword(password, saltBase64);

            var expectedHashBytes = Convert.FromBase64String(expectedHashBase64);

            var computedHashBytes = Convert.FromBase64String(computedHashBase64);

            return CryptographicOperations.FixedTimeEquals(expectedHashBytes, computedHashBytes);
        }
    }
}