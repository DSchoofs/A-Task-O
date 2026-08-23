using Microsoft.Extensions.Options;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Settings;
using Schoofs_ATaskO.ViewModels;
using System.Security.Cryptography;
using System.Text.Json;

namespace Schoofs_ATaskO.Services
{
    public class SetupService : ISetupService
    {
        private readonly IDatabaseSetupService _databaseSetupService;
        private readonly IUserService _userService;
        private readonly SecuritySettings _securitySettings;
        private readonly string _securityFilePath;

        public string ErrorMessage { get; private set; } = string.Empty;

        public SetupService(IDatabaseSetupService databaseSetupService, IUserService userService, IOptions<SecuritySettings> securityOptions, IHostEnvironment environment)
        {
            _databaseSetupService = databaseSetupService;
            _userService = userService;
            _securitySettings = securityOptions.Value;
            _securityFilePath = Path.Combine(environment.ContentRootPath, "App_Data", "security.json");
        }

        public bool HasPasswordPepper()
        {
            return !string.IsNullOrWhiteSpace(_securitySettings.PasswordPepper);
        }

        public async Task<bool> IsDatabaseReadyAsync()
        {
            ErrorMessage = string.Empty;

            var status = await _databaseSetupService.GetStatusAsync();

            if (!status.IsFullyReady)
            {
                ErrorMessage = _databaseSetupService.ErrorMessage;
            }

            return status.IsFullyReady;
        }

        public string GeneratePasswordPepper()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        public bool CreatePasswordPepper(string pepper)
        {
            ErrorMessage = string.Empty;

            if (HasPasswordPepper())
            {
                ErrorMessage = "Es ist bereits ein PasswordPepper konfiguriert.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(pepper))
            {
                ErrorMessage = "Der PasswordPepper darf nicht leer sein.";

                return false;
            }

            if (pepper.Length < 12)
            {
                ErrorMessage = "Der persönliche PasswordPepper muss mindestens 12 Zeichen lang sein.";

                return false;
            }

            try
            {
                var securityConfiguration = new
                {
                    Security = new
                    {
                        PasswordPepper = pepper
                    }
                };

                var json = JsonSerializer.Serialize(securityConfiguration, new JsonSerializerOptions { WriteIndented = true });

                var directory = Path.GetDirectoryName(_securityFilePath);

                if (string.IsNullOrWhiteSpace(directory))
                {
                    ErrorMessage = "Der Ordner für die Security-Konfiguration konnte nicht bestimmt werden.";

                    return false;
                }

                Directory.CreateDirectory(directory);

                File.WriteAllText(_securityFilePath, json);

                Console.WriteLine("--------------------------------------------------------------------------");
                Console.WriteLine("Die Sicherheitskonfiguration wurde gespeichert.");
                Console.WriteLine("Bitte schließen Sie die Anwendung mit Strg + C und starten Sie sie erneut.");
                Console.WriteLine("--------------------------------------------------------------------------");

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Der PasswordPepper konnte nicht gespeichert werden: {ex.Message}";

                return false;
            }
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