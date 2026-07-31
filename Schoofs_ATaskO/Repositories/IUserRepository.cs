using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Repositories
{
    public interface IUserRepository
    {
        List<User> GetAllUsers();
        User? GetUserById(int id);
        bool AddUser(User user);
        bool UpdateUser(User user);
        bool DeleteUserById(int id);
        string ErrorMessage { get; set; }
    }
}
