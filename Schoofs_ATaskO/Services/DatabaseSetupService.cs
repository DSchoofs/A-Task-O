using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Repositories;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services
{
    public class DatabaseSetupService : IDatabaseSetupService
    {
        private readonly IDatabaseRepository _databaseRepository;
        private readonly IDatabaseConfigurationStorage _configurationStorage;

        public string ErrorMessage { get; private set; } = string.Empty;

        public DatabaseSetupService(IDatabaseRepository databaseRepository, IDatabaseConfigurationStorage configurationStorage)
        {
            _databaseRepository = databaseRepository;
            _configurationStorage = configurationStorage;
        }

        public async Task<DatabaseSetupStatus> GetStatusAsync()
        {
            ErrorMessage = string.Empty;

            var repositoryStatus =
                await _databaseRepository.GetStatusAsync();

            if (!string.IsNullOrWhiteSpace(
                _databaseRepository.ErrorMessage))
            {
                ErrorMessage =
                    _databaseRepository.ErrorMessage;
            }

            var status = new DatabaseSetupStatus
            {
                ConfigurationFileExists =
                    repositoryStatus.ConfigurationFileExists,

                ConfigurationIsValid =
                    repositoryStatus.ConfigurationIsValid,

                HasConnectionString =
                    repositoryStatus.HasConnectionString,

                IsServerReachable =
                    repositoryStatus.IsServerReachable,

                IsDatabaseReachable =
                    repositoryStatus.IsDatabaseReachable,

                HasPendingMigrations =
                    repositoryStatus.HasPendingMigrations,

                StatusMessage =
                    repositoryStatus.StatusMessage
            };

            return status;
        }
        public async Task<bool> TestLocalSqlServerAsync()
        {
            ErrorMessage = string.Empty;

            var localConfiguration = new DatabaseConfigurationViewModel
            {
                UseDefaultConfiguration = true,
                ServerName = @"(localdb)\MSSQLLocalDB",
                AuthenticationMode = "Windows"
            };

            var result = await _databaseRepository.TestServerConnectionAsync(localConfiguration);

            if (!result)
            {
                ErrorMessage = _databaseRepository.ErrorMessage;
            }

            return result;
        }
        public async Task<bool> TestServerConnectionAsync(DatabaseConfigurationViewModel model)
        {
            ErrorMessage = string.Empty;

            var result =await _databaseRepository.TestServerConnectionAsync(model);

            if (!result)
            {
                ErrorMessage = _databaseRepository.ErrorMessage;
            }

            return result;
        }

        public async Task<bool> SaveConfigurationAsync(DatabaseConfigurationViewModel model)
        {
            ErrorMessage = string.Empty;

            var serverConnectionSuccessful = await _databaseRepository.TestServerConnectionAsync(model);

            if (!serverConnectionSuccessful)
            {
                ErrorMessage =_databaseRepository.ErrorMessage;

                return false;
            }

            var databaseCreated = await _databaseRepository.EnsureDatabaseExistsAsync(model);

            if (!databaseCreated)
            {
                ErrorMessage = _databaseRepository.ErrorMessage;

                return false;
            }

            var databaseConnectionSuccessful = await _databaseRepository.TestDatabaseConnectionAsync(model);

            if (!databaseConnectionSuccessful)
            {
                ErrorMessage = _databaseRepository.ErrorMessage;

                return false;
            }

            var databaseConnectionString = BuildDatabaseConnectionString(model);

            try
            {
                await _configurationStorage.SaveConnectionStringAsync(databaseConnectionString);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Die Datenbank wurde vorbereitet, aber die Konfiguration konnte nicht gespeichert werden: {ex.Message}";

                return false;
            }

            return true;
        }

        public async Task<bool> ApplyMigrationsAsync(DatabaseConfigurationViewModel model)
        {
            ErrorMessage = string.Empty;

            var result = await _databaseRepository.ApplyMigrationsAsync(model);

            if (!result)
            {
                ErrorMessage = _databaseRepository.ErrorMessage;
            }

            return result;
        }

        private string BuildDatabaseConnectionString(DatabaseConfigurationViewModel model)
        {
            var serverConnectionString = BuildServerConnectionString(model);

            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(serverConnectionString);

            builder.InitialCatalog = "ATaskO";

            return builder.ConnectionString;
        }

        private string BuildServerConnectionString(
            DatabaseConfigurationViewModel model)
        {
            if (model.UseDefaultConfiguration)
            {
                return @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True;";
            }

            if (string.IsNullOrWhiteSpace(model.ServerName))
            {
                throw new InvalidOperationException("Bitte einen Server oder eine Instanz angeben.");
            }

            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
                {
                    DataSource = model.ServerName,
                    TrustServerCertificate = true
                };

            if (model.AuthenticationMode == "SqlServer")
            {
                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                {
                    throw new InvalidOperationException("Für die SQL-Server-Authentifizierung werden Benutzername und Passwort benötigt.");
                }

                builder.UserID = model.Username;
                builder.Password = model.Password;
                builder.IntegratedSecurity = false;
            }
            else
            {
                builder.IntegratedSecurity = true;
            }

            return builder.ConnectionString;
        }
    }
}