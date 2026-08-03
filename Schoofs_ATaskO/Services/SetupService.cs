using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services
{
    public class SetupService : ISetupService
    {
        private readonly IUserService _userService;

        public string ErrorMessage { get; private set; } = string.Empty;

        public SetupService(IUserService userService)
        {
            _userService = userService;
        }

        public bool HasAdminUser()
        {
            ErrorMessage = string.Empty;

            var result = _userService.HasAdminUser();

            if (!result && !string.IsNullOrWhiteSpace(_userService.ErrorMessage))
            {
                ErrorMessage = _userService.ErrorMessage;
            }

            return result;
        }

        public bool IsInitialSetupRequired()
        {
            return !HasAdminUser();
        }

        public bool CreateInitialAdmin(InitialAdminViewModel model)
        {
            ErrorMessage = string.Empty;

            if (HasAdminUser())
            {
                ErrorMessage = "Es existiert bereits ein Administrator.";
                return false;
            }

            var user = new User
            {
                FirstName = "Initial",
                LastName = "Administrator",
                Email = model.Email,
                IsAdmin = true
            };

            var result = _userService.CreateUser(user, model.Password);

            if (!result)
            {
                ErrorMessage = _userService.ErrorMessage;
                return false;
            }

            return true;
        }
    }
}