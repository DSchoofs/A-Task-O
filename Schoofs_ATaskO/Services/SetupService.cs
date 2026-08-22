using Microsoft.Extensions.Options;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Settings;
using Schoofs_ATaskO.ViewModels;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Schoofs_ATaskO.Services
{
    public class SetupService : ISetupService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly SecuritySettings _securitySettings;
        private readonly string _securityFilePath;

        public string ErrorMessage { get; private set; } = string.Empty;

        public SetupService(IUserService userService, IOptions<SecuritySettings> securityOptions, IHostEnvironment environment,
    IConfiguration configuration)
        {
            _userService = userService;
            _securitySettings = securityOptions.Value;
            _configuration = configuration;
            _securityFilePath = Path.Combine(environment.ContentRootPath, "App_Data", "security.json");
        }

        public bool HasPasswordPepper()
        {
            return !string.IsNullOrWhiteSpace(_securitySettings.PasswordPepper);
        }
        public async Task<bool> IsDatabaseReadyAsync()
        {
            var connectionString = _configuration.GetConnectionString("ATaskO");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

                optionsBuilder.UseSqlServer(connectionString);

                await using var dbContext = new ApplicationDbContext(optionsBuilder.Options);

                var canConnect = await dbContext.Database.CanConnectAsync();

                if (!canConnect)
                {
                    return false;
                }

                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

                return !pendingMigrations.Any();
            }
            catch
            {
                return false;
            }
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
                ErrorMessage =
                    "Es ist bereits ein PasswordPepper konfiguriert.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(pepper))
            {
                ErrorMessage =
                    "Der PasswordPepper darf nicht leer sein.";

                return false;
            }

            if (pepper.Length < 12)
            {
                ErrorMessage =
                    "Der persönliche PasswordPepper muss mindestens 12 Zeichen lang sein.";

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

                var json = JsonSerializer.Serialize(securityConfiguration, new JsonSerializerOptions{ WriteIndented = true });

                var directory = Path.GetDirectoryName(_securityFilePath);

                if (string.IsNullOrWhiteSpace(directory))
                {
                    ErrorMessage = "Der Ordner für die Security-Konfiguration konnte nicht bestimmt werden.";

                    return false;
                }

                Directory.CreateDirectory(directory);

                File.WriteAllText(_securityFilePath,json);

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