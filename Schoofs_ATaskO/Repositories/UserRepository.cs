using Dapper;
using Microsoft.Data.SqlClient;
using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        public string sqlcmd;
        public string Fehlermeldung { get; set; }
        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Atasko")!;
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
                Fehlermeldung = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                Fehlermeldung = $"Unexpected Error: {ex.Message}";
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
                Fehlermeldung = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                Fehlermeldung = $"Unexpected Error: {ex.Message}";
            }
            return null;
        }

        public int AddUser(User user)
        {
            try
            {
                sqlcmd = @"INSERT INTO Users (FirstName, LastName, Email, PasswordHash, IsAdmin, LastLogin, CompletedTasks) OUTPUT INSERTED.UserId VALUES (@FirstName, @LastName, @Email, @PasswordHash, @IsAdmin, @LastLogin, @CompletedTasks)";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var userId = conn.QuerySingle<int>(sqlcmd, user);
                    return userId;
                }
            }
            catch (SqlException sqlex)
            {
                Fehlermeldung = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                Fehlermeldung = $"Unexpected Error: {ex.Message}";
            }
            return 0;
        }

        public bool UpdateUser(User user)
        {
            try
            {
                sqlcmd = @"UPDATE Users SET FirstName = @FirstName, LastName = @LastName, Email = @Email, PasswordHash = @PasswordHash, IsAdmin = @IsAdmin, LastLogin = @LastLogin, CompletedTasks = @CompletedTasks WHERE UserId = @UserId";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var rowsAffected = conn.Execute(sqlcmd, user);
                    return rowsAffected > 0;
                }
            }
            catch (SqlException sqlex)
            {
                Fehlermeldung = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                Fehlermeldung = $"Unexpected Error: {ex.Message}";
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
                Fehlermeldung = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                Fehlermeldung = $"Unexpected Error: {ex.Message}";
            }
            return false;
        }
    }
}