using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Settings;
using System.Security.Cryptography;

namespace Schoofs_ATaskO.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SecuritySettings _securitySettings;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public string sqlcmd;
        public string ErrorMessage { get; set; }
        public UserRepository(IConfiguration configuration, IOptions<SecuritySettings> securityOptions)
        {
            _securitySettings = securityOptions.Value;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("Atasko")!;
            if (string.IsNullOrWhiteSpace(_securitySettings.PasswordPepper))
            {
                throw new InvalidOperationException("PasswordPepper ist nicht konfiguriert.");
            }
        }

        public List<User> GetAllUsers()
        {
            try
            {
                List<User> users = new List<User>();
                sqlcmd = "SELECT UserId, FirstName, LastName, Email, PasswordHash, IsAdmin, LastLogin, CompletedTasks FROM Users";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    users = conn.Query<User>(sqlcmd).ToList();
                    return users;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }
            return new List<User>();
        }

        public User? GetUserById(int id)
        {
            try
            {
                User? user = null;
                sqlcmd = "SELECT UserId, FirstName, LastName, Email, PasswordHash, IsAdmin, LastLogin, CompletedTasks FROM Users WHERE UserId = @Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    user = conn.QuerySingleOrDefault<User>(sqlcmd, new { Id = id });
                    return user;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }
            return null;
        }

        public bool AddUser(User user)
        {
            try
            {
                var pepper = _configuration["PasswordPepper"];
                var salt = GenerateSalt();
                var hash = HashPassword(user.PasswordHash, salt);

                user.PasswordSalt = salt;
                user.PasswordHash = hash;

                sqlcmd = @"INSERT INTO Users (FirstName, LastName, Email, PasswordHash, PasswordSalt, IsAdmin, LastLogin, CompletedTasks) VALUES (@FirstName, @LastName, @Email, @PasswordHash, @PasswordSalt, @IsAdmin, @LastLogin, @CompletedTasks)";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var rowsAffected = conn.Execute(sqlcmd, user);
                    return rowsAffected > 0;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }

            return false;
        }

        public bool UpdateUser(User user)
        {
            try
            {
                sqlcmd = @"UPDATE Users SET FirstName = @FirstName, LastName = @LastName, Email = @Email, PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt, IsAdmin = @IsAdmin, LastLogin = @LastLogin, CompletedTasks = @CompletedTasks WHERE UserId = @UserId";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var rowsAffected = conn.Execute(sqlcmd, user);
                    return rowsAffected > 0;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }
            return false;
        }

        public bool DeleteUserById(int id)
        {
            try
            {
                sqlcmd = "DELETE FROM Users WHERE UserId = @Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var rowsAffected = conn.Execute(sqlcmd, new { Id = id });
                    return rowsAffected > 0;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }
            return false;
        }

        public User? GetUserByEmail(string email)
        {
            try
            {
                sqlcmd = @"SELECT UserId, FirstName, LastName, Email, PasswordHash, PasswordSalt, IsAdmin, LastLogin, CompletedTasks FROM Users WHERE Email = @Email";
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                return conn.QuerySingleOrDefault<User>(sqlcmd, new { Email = email });
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }

            return null;
        }

        private string HashPassword(string password, string saltBase64)
        {
            var saltBytes = Convert.FromBase64String(saltBase64);
            var passwordWithPepper = password + _securitySettings.PasswordPepper;

            var hashBytes = KeyDerivation.Pbkdf2(password: passwordWithPepper, salt: saltBytes, prf: KeyDerivationPrf.HMACSHA256, iterationCount: 100000, numBytesRequested: 32);

            return Convert.ToBase64String(hashBytes);
        }

        private string GenerateSalt()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        }
        public bool ValidatePassword(User user, string password)
        {
            try
            {
                var passwordWithPepper = password + _securitySettings.PasswordPepper;
                var saltBytes = Convert.FromBase64String(user.PasswordSalt);

                var hashBytes = KeyDerivation.Pbkdf2(
                    password: passwordWithPepper,
                    salt: saltBytes,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 32);

                var computedHash = Convert.ToBase64String(hashBytes);
                return computedHash == user.PasswordHash;
            }
            catch
            {
                return false;
            }
        }
    }

}