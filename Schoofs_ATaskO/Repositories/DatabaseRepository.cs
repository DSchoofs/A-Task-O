using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.ViewModels;
using System.Text.Json;

namespace Schoofs_ATaskO.Repositories
{
    public class DatabaseRepository : IDatabaseRepository
    {
        private const string DatabaseName = "ATaskO";

        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public string ErrorMessage { get; set; } = string.Empty;

        public DatabaseRepository(IConfiguration configuration, ApplicationDbContext dbContext, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _environment = environment;
        }
        public async Task<bool> TestServerConnectionAsync(DatabaseConfigurationViewModel model)
        {
            ErrorMessage = string.Empty;

            try
            {
                var connectionString = BuildServerConnectionString(model);

                await using var connection = new SqlConnection(connectionString);

                await connection.OpenAsync();

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Der SQL-Server konnte nicht erreicht werden: {ex.Message}";

                return false;
            }
        }
        public async Task<bool> EnsureDatabaseExistsAsync(DatabaseConfigurationViewModel model)
        {
            ErrorMessage = string.Empty;

            try
            {
                var serverConnectionString = BuildServerConnectionString(model);

                await using var connection = new SqlConnection(serverConnectionString);

                await connection.OpenAsync();

                var databaseExists = await DoesDatabaseExistAsync(connection, DatabaseName);

                if (!databaseExists)
                {
                    await CreateDatabaseAsync(connection, DatabaseName);
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Die Datenbank {DatabaseName} konnte nicht erstellt werden: {ex.Message}";

                return false;
            }
        }
        public async Task<bool> TestDatabaseConnectionAsync(DatabaseConfigurationViewModel model)
        {
            ErrorMessage = string.Empty;

            try
            {
                var connectionString = BuildDatabaseConnectionString(model);

                await using var connection = new SqlConnection(connectionString);

                await connection.OpenAsync();

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Die Datenbank {DatabaseName} konnte nicht erreicht werden: {ex.Message}";

                return false;
            }
        }
        public async Task<bool> ApplyMigrationsAsync(DatabaseConfigurationViewModel model)
        {
            ErrorMessage = string.Empty;

            try
            {
                var connectionString = BuildDatabaseConnectionString(model);

                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

                optionsBuilder.UseSqlServer(connectionString);

                await using var dbContext = new ApplicationDbContext(optionsBuilder.Options);

                await dbContext.Database.MigrateAsync();

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Die Migrationen konnten nicht angewendet werden: {ex.Message}";

                return false;
            }
        }
        public async Task<DatabaseRepositoryStatus> GetStatusAsync()
        {
            ErrorMessage = string.Empty;

            var configurationFilePath =
                Path.Combine(
                    _environment.ContentRootPath,
                    "App_Data",
                    "database.json");

            if (!File.Exists(configurationFilePath))
            {
                return new DatabaseRepositoryStatus
                {
                    ConfigurationFileExists = false,
                    ConfigurationIsValid = false,
                    HasConnectionString = false,
                    IsServerReachable = false,
                    IsDatabaseReachable = false,
                    HasPendingMigrations = true,
                    StatusMessage =
                        "Es wurde noch keine Datenbankkonfiguration gespeichert."
                };
            }

            string? connectionString;

            try
            {
                var configurationJson =
                    await File.ReadAllTextAsync(
                        configurationFilePath);

                using var document =
                    JsonDocument.Parse(configurationJson);

                if (!document.RootElement.TryGetProperty(
                        "ConnectionStrings",
                        out var connectionStringsElement) ||
                    connectionStringsElement.ValueKind !=
                        JsonValueKind.Object)
                {
                    return CreateInvalidConfigurationStatus(
                        "Die database.json enthält keinen gültigen " +
                        "Connection-Strings-Abschnitt.");
                }

                if (!connectionStringsElement.TryGetProperty(
                        "ATaskO",
                        out var connectionStringElement) ||
                    connectionStringElement.ValueKind !=
                        JsonValueKind.String)
                {
                    return CreateInvalidConfigurationStatus(
                        "Die database.json enthält keinen gültigen " +
                        "Connection String für ATaskO.");
                }

                connectionString =
                    connectionStringElement.GetString();

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return CreateInvalidConfigurationStatus(
                        "Der Connection String in der database.json ist leer.");
                }
            }
            catch (JsonException)
            {
                return CreateInvalidConfigurationStatus(
                    "Die database.json enthält ungültiges JSON.");
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    $"Die database.json konnte nicht gelesen werden: {ex.Message}";

                return CreateInvalidConfigurationStatus(
                    "Die database.json konnte nicht gelesen werden.");
            }

            SqlConnectionStringBuilder connectionStringBuilder;

            try
            {
                connectionStringBuilder =
                    new SqlConnectionStringBuilder(
                        connectionString);
            }
            catch (ArgumentException ex)
            {
                ErrorMessage =
                    $"Der Connection String ist ungültig: {ex.Message}";

                return CreateInvalidConfigurationStatus(
                    "Der Connection String ist syntaktisch ungültig.");
            }

            var databaseName =
                connectionStringBuilder.InitialCatalog;

            var serverConnectionString =
                RemoveDatabaseName(
                    connectionString);

            var isServerReachable =
                await TestServerConnectionAsync(
                    serverConnectionString);

            if (!isServerReachable)
            {
                return new DatabaseRepositoryStatus
                {
                    ConfigurationFileExists = true,
                    ConfigurationIsValid = false,
                    HasConnectionString = true,
                    IsServerReachable = false,
                    IsDatabaseReachable = false,
                    HasPendingMigrations = true,
                    StatusMessage =
                        "Der konfigurierte SQL-Server ist nicht erreichbar."
                };
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return new DatabaseRepositoryStatus
                {
                    ConfigurationFileExists = true,
                    ConfigurationIsValid = true,
                    HasConnectionString = true,
                    IsServerReachable = true,
                    IsDatabaseReachable = false,
                    HasPendingMigrations = true,
                    StatusMessage =
                        "Der SQL-Server ist erreichbar, aber es wurde kein " +
                        "Datenbankname angegeben."
                };
            }

            var isDatabaseReachable =
                await TestStoredDatabaseConnectionAsync(
                    connectionString);

            if (!isDatabaseReachable)
            {
                return new DatabaseRepositoryStatus
                {
                    ConfigurationFileExists = true,
                    ConfigurationIsValid = true,
                    HasConnectionString = true,
                    IsServerReachable = true,
                    IsDatabaseReachable = false,
                    HasPendingMigrations = true,
                    StatusMessage =
                        $"Der SQL-Server ist erreichbar, aber die Datenbank " +
                        $"'{databaseName}' ist nicht erreichbar."
                };
            }

            var hasPendingMigrations = true;

            try
            {
                var pendingMigrations =
                    await _dbContext.Database
                        .GetPendingMigrationsAsync();

                hasPendingMigrations =
                    pendingMigrations.Any();
            }
            catch (Exception ex)
            {
                ErrorMessage =
                    $"Die Migrationen konnten nicht geprüft werden: {ex.Message}";

                return new DatabaseRepositoryStatus
                {
                    ConfigurationFileExists = true,
                    ConfigurationIsValid = true,
                    HasConnectionString = true,
                    IsServerReachable = true,
                    IsDatabaseReachable = true,
                    HasPendingMigrations = true,
                    StatusMessage =
                        "Die Datenbank ist erreichbar, aber der " +
                        "Migrationsstatus konnte nicht geprüft werden."
                };
            }

            return new DatabaseRepositoryStatus
            {
                ConfigurationFileExists = true,
                ConfigurationIsValid = true,
                HasConnectionString = true,
                IsServerReachable = true,
                IsDatabaseReachable = true,
                HasPendingMigrations = hasPendingMigrations,
                StatusMessage = hasPendingMigrations
                    ? "Die Datenbank ist erreichbar, aber es stehen noch " +
                      "Migrationen aus."
                    : "Die Datenbank ist vollständig eingerichtet."
            };
        }
        private async Task<bool> DoesDatabaseExistAsync(SqlConnection connection, string databaseName)
        {
            const string sql = "SELECT COUNT(1) FROM sys.databases WHERE name = @DatabaseName";

            var databaseCount = await connection.ExecuteScalarAsync<int>(sql, new { DatabaseName = databaseName });

            return databaseCount > 0;
        }
        private async Task CreateDatabaseAsync(SqlConnection connection, string databaseName)
        {
            var escapedDatabaseName = databaseName.Replace("]", "]]");

            var sql = $"CREATE DATABASE [{escapedDatabaseName}]";

            await connection.ExecuteAsync(sql);
        }
        private async Task<bool> TestStoredDatabaseConnectionAsync(string connectionString)
        {
            try
            {
                await using var connection =
                    new SqlConnection(
                        connectionString);

                await connection.OpenAsync();

                return true;
            }
            catch (SqlException ex)
            {
                ErrorMessage =
                    $"Die Datenbankverbindung konnte nicht geöffnet werden: " +
                    $"{ex.Message}";

                return false;
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage =
                    $"Der Datenbank-Connection-String ist ungültig: " +
                    $"{ex.Message}";

                return false;
            }
        }
        private string BuildServerConnectionString(DatabaseConfigurationViewModel model)
        {
            if (model.UseDefaultConfiguration)
            {
                return @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True;";
            }

            if (string.IsNullOrWhiteSpace(
                model.ServerName))
            {
                throw new InvalidOperationException("Bitte einen Server oder eine Instanz angeben.");
            }

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = model.ServerName,
                TrustServerCertificate = true
            };

            ConfigureAuthentication(builder, model);

            return builder.ConnectionString;
        }
        private string BuildDatabaseConnectionString(DatabaseConfigurationViewModel model)
        {
            var builder = new SqlConnectionStringBuilder(BuildServerConnectionString(model))
            {
                InitialCatalog = DatabaseName
            };

            return builder.ConnectionString;
        }
        private void ConfigureAuthentication(SqlConnectionStringBuilder builder, DatabaseConfigurationViewModel model)
        {
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
        }
        private async Task<bool> TestServerConnectionAsync(string connectionString)
        {
            try
            {
                await using var connection = new SqlConnection(connectionString);

                await connection.OpenAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
        private string RemoveDatabaseName(string connectionString)
        {
            var builder =
                new SqlConnectionStringBuilder(
                    connectionString);

            builder.Remove("Initial Catalog");
            builder.Remove("Database");

            return builder.ConnectionString;
        }
        private static DatabaseRepositoryStatus
    CreateInvalidConfigurationStatus(
        string message)
        {
            return new DatabaseRepositoryStatus
            {
                ConfigurationFileExists = true,
                ConfigurationIsValid = false,
                HasConnectionString = false,
                IsServerReachable = false,
                IsDatabaseReachable = false,
                HasPendingMigrations = true,
                StatusMessage = message
            };
        }
    }
}