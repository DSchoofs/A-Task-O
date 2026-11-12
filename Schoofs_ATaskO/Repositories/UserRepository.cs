using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Settings;

namespace Schoofs_ATaskO.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SecuritySettings _securitySettings;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public string sqlcmd = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public UserRepository(IConfiguration configuration, IOptions<SecuritySettings> securityOptions)
        {
            _securitySettings = securityOptions.Value;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("ATaskO")!;

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
                sqlcmd = "SELECT UserId, FirstName, LastName, Email, PasswordHash, PasswordSalt, IsAdmin, LastLogin, CompletedTasks FROM Users WHERE UserId = @Id";
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
                sqlcmd = @"INSERT INTO Users (FirstName, LastName, Email, PasswordHash, PasswordSalt, IsAdmin) VALUES (@FirstName, @LastName, @Email, @PasswordHash, @PasswordSalt, @IsAdmin)";

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
            ErrorMessage = string.Empty;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        sqlcmd = "UPDATE TaskBlocks SET Status = @FreeStatus, AssignedByUserId = NULL, AssignedAt = NULL, CompletedByUserId = NULL, CompletedAt = NULL WHERE AssignedByUserId = @Id AND Status <> @CompletedStatus";

                        conn.Execute(sqlcmd, new
                        {
                            Id = id,
                            FreeStatus = (int)TaskBlockStatus.Free,
                            CompletedStatus = (int)TaskBlockStatus.Completed
                        }, transaction);

                        sqlcmd = "UPDATE TaskBlocks SET AssignedByUserId = NULL WHERE AssignedByUserId = @Id";

                        conn.Execute(sqlcmd, new { Id = id }, transaction);

                        sqlcmd = "UPDATE TaskBlocks SET CompletedByUserId = NULL WHERE CompletedByUserId = @Id";

                        conn.Execute(sqlcmd, new { Id = id }, transaction);

                        sqlcmd = "UPDATE TaskItems SET CreatedByUserId = NULL WHERE CreatedByUserId = @Id";

                        conn.Execute(sqlcmd, new { Id = id }, transaction);

                        sqlcmd = "UPDATE TaskItems SET ModifiedByUserId = NULL WHERE ModifiedByUserId = @Id";

                        conn.Execute(sqlcmd, new { Id = id }, transaction);

                        sqlcmd = "DELETE FROM Users WHERE UserId = @Id";

                        var rowsAffected = conn.Execute(sqlcmd, new { Id = id }, transaction);

                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();

                            ErrorMessage = "Der Benutzer wurde nicht gefunden.";

                            return false;
                        }

                        transaction.Commit();

                        return true;
                    }
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
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var user = conn.QuerySingleOrDefault<User>(sqlcmd, new { Email = email });
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
        public bool HasAdminUser()
        {
            try
            {
                sqlcmd = $"SELECT COUNT(1) FROM Users WHERE IsAdmin = 1";
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var adminCount = connection.ExecuteScalar<int>(sqlcmd);
                    return adminCount > 0;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
                return false;
            }
        }
        public bool UpdateLastLogin(int userId, DateTime lastLogin)
        {
            try
            {
                sqlcmd = "UPDATE Users SET LastLogin = @LastLogin WHERE UserId = @UserId";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var rowsAffected = conn.Execute(sqlcmd, new
                    {
                        UserId = userId,
                        LastLogin = lastLogin
                    });

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
        public bool UpdateCompletedTasks(int userId, int change)
        {
            try
            {
                sqlcmd = "UPDATE Users SET CompletedTasks = CASE WHEN ISNULL(CompletedTasks, 0) + @Change < 0 THEN 0 ELSE ISNULL(CompletedTasks, 0) + @Change END WHERE UserId = @UserId";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    var rowsAffected = conn.Execute(sqlcmd, new { UserId = userId, Change = change });

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
    }

}