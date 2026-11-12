using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Services
{
    public interface IUserService
    {
        bool CreateUser(User user, string plainPassword);
        User? AuthenticateUser(string email, string plainPassword);
        User? GetUserById(int id);
        User? GetUserByEmail(string email);
        List<User> GetAllUsers();
        bool UpdateUser(User user);
        bool DeleteUserById(int id);
        string ErrorMessage { get; }
        bool HasAdminUser();
        string GetUserDisplayName(int? userId, string missingUserText = "Nicht bekannt");
        bool UpdateCompletedTasks(int userId, int change);
    }
}