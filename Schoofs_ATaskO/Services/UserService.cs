using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Repositories;

namespace Schoofs_ATaskO.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;

        public string ErrorMessage { get; private set; } = string.Empty;

        public UserService(IUserRepository userRepository, IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
        }

        public bool CreateUser(User user, string plainPassword)
        {
            try
            {
                var existingUser = _userRepository.GetUserByEmail(user.Email);
                if (existingUser != null)
                {
                    ErrorMessage = "Es existiert bereits ein Benutzer mit dieser E-Mail-Adresse.";
                    return false;
                }

                var salt = _passwordService.GenerateSalt();
                var hash = _passwordService.HashPassword(plainPassword, salt);

                user.PasswordSalt = salt;
                user.PasswordHash = hash;

                var result = _userRepository.AddUser(user);

                if (!result)
                {
                    ErrorMessage = _userRepository.ErrorMessage;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
                return false;
            }
        }

        public User? AuthenticateUser(string email, string plainPassword)
        {
            try
            {
                var user = _userRepository.GetUserByEmail(email);

                if (user == null)
                {
                    ErrorMessage = "Ungültige E-Mail oder ungültiges Passwort.";
                    return null;
                }

                var isValid = _passwordService.VerifyPassword(
                    plainPassword,
                    user.PasswordSalt,
                    user.PasswordHash);

                if (!isValid)
                {
                    ErrorMessage = "Ungültige E-Mail oder ungültiges Passwort.";
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
                return null;
            }
        }

        public User? GetUserById(int id)
        {
            var user = _userRepository.GetUserById(id);

            if (user == null && !string.IsNullOrWhiteSpace(_userRepository.ErrorMessage))
            {
                ErrorMessage = _userRepository.ErrorMessage;
            }

            return user;
        }

        public User? GetUserByEmail(string email)
        {
            var user = _userRepository.GetUserByEmail(email);

            if (user == null && !string.IsNullOrWhiteSpace(_userRepository.ErrorMessage))
            {
                ErrorMessage = _userRepository.ErrorMessage;
            }

            return user;
        }

        public List<User> GetAllUsers()
        {
            var users = _userRepository.GetAllUsers();

            if (users.Count == 0 && !string.IsNullOrWhiteSpace(_userRepository.ErrorMessage))
            {
                ErrorMessage = _userRepository.ErrorMessage;
            }

            return users;
        }

        public bool UpdateUser(User user)
        {
            var result = _userRepository.UpdateUser(user);

            if (!result)
            {
                ErrorMessage = _userRepository.ErrorMessage;
            }

            return result;
        }

        public bool DeleteUserById(int id)
        {
            var result = _userRepository.DeleteUserById(id);

            if (!result)
            {
                ErrorMessage = _userRepository.ErrorMessage;
            }

            return result;
        }
    }
}