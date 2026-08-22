using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.ViewModels;
using System.Text.Json;
using Schoofs_ATaskO.Helpers;

namespace Schoofs_ATaskO.Repositories
{
    public class DatabaseRepository : IDatabaseRepository
    {
        private readonly IWebHostEnvironment _environment;
        public string ErrorMessage { get; set; } = string.Empty;

        public DatabaseRepository(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public async Task<bool> TestServerConnectionAsync(DatabaseConfigurationViewModel model)
        {
            ErrorMessage = string.Empty;

            try
            {
                var connectionString = DatabaseHelper.BuildServerConnectionString(model);

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
                var serverConnectionString = DatabaseHelper.BuildServerConnectionString(model);

                await using var connection = new SqlConnection(serverConnectionString);

                await connection.OpenAsync();

                const string databaseName = "ATaskO";

                var databaseExists = await DoesDatabaseExistAsync(connection, databaseName);

                if (!databaseExists)
                {
                    await CreateDatabaseAsync(connection,databaseName);
                }

                var databaseConnectionString = DatabaseHelper.BuildDatabaseConnectionString(
                            model);

                var databaseReachable = await DatabaseHelper.WaitForDatabaseConnectionAsync(databaseConnectionString);

                if (!databaseReachable)
                {
                    ErrorMessage = $"Die Datenbank {databaseName} wurde erstellt, konnte danach aber nicht erreicht werden.";

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Die Datenbank ATaskO konnte nicht erstellt werden: {ex.Message}";

                return false;
            }
        }

        public async Task<bool> ApplyMigrationsAsync(DatabaseConfigurationViewModel model)
        {
            ErrorMessage = string.Empty;

            try
            {
                var connectionString = DatabaseHelper.BuildDatabaseConnectionString(model);

                var migrationsStarted = await DatabaseHelper.ApplyMigrationsWithRetryAsync(connectionString);

                if (!migrationsStarted)
                {
                    ErrorMessage = "Die Migrationen konnten nicht gestartet werden.";

                    return false;
                }

                var migrationsApplied = await DatabaseHelper.WaitForMigrationsAsync(connectionString);

                if (!migrationsApplied)
                {
                    ErrorMessage = "Die Migrationen wurden gestartet, konnten danach aber nicht als abgeschlossen bestätigt werden.";

                    return false;
                }

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

            var configurationFilePath =Path.Combine(_environment.ContentRootPath,"App_Data","database.json");

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
                    StatusMessage = "Es wurde noch keine Datenbankkonfiguration gespeichert."
                };
            }

            string? connectionString;

            try
            {
                var configurationJson = await File.ReadAllTextAsync(configurationFilePath);

                using var document =
                    JsonDocument.Parse(configurationJson);

                if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStringsElement) || connectionStringsElement.ValueKind != JsonValueKind.Object)
                {
                    return CreateInvalidConfigurationStatus("Die database.json enthält keinen gültigen Connection-Strings-Abschnitt.");
                }

                if (!connectionStringsElement.TryGetProperty("ATaskO", out var connectionStringElement) || connectionStringElement.ValueKind !=JsonValueKind.String)
                {
                    return CreateInvalidConfigurationStatus("Die database.json enthält keinen gültigen Connection String für ATaskO.");
                }

                connectionString =connectionStringElement.GetString();

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return CreateInvalidConfigurationStatus("Der Connection String in der database.json ist leer.");
                }
            }
            catch (JsonException)
            {
                return CreateInvalidConfigurationStatus("Die database.json enthält ungültiges JSON.");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Die database.json konnte nicht gelesen werden: {ex.Message}";

                return CreateInvalidConfigurationStatus("Die database.json konnte nicht gelesen werden.");
            }

            SqlConnectionStringBuilder connectionStringBuilder;

            try
            {
                connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            }
            catch (ArgumentException ex)
            {
                ErrorMessage = $"Der Connection String ist ungültig: {ex.Message}";

                return CreateInvalidConfigurationStatus("Der Connection String ist syntaktisch ungültig.");
            }

            var databaseName = connectionStringBuilder.InitialCatalog;

            var serverConnectionString = DatabaseHelper.RemoveDatabaseName(connectionString);

            var isServerReachable = await TestConnectionStringAsync(serverConnectionString);

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
                    StatusMessage = "Der konfigurierte SQL-Server ist nicht erreichbar."
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
                    StatusMessage = "Der SQL-Server ist erreichbar, aber es wurde kein Datenbankname angegeben."
                };
            }

            var isDatabaseReachable = await TestConnectionStringAsync(connectionString);

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
                    StatusMessage = $"Der SQL-Server ist erreichbar, aber die Datenbank '{databaseName}' ist nicht erreichbar."
                };
            }

            var hasPendingMigrations = true;

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

                optionsBuilder.UseSqlServer(connectionString);

                await using var dbContext = new ApplicationDbContext(optionsBuilder.Options);

                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

                hasPendingMigrations = pendingMigrations.Any();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Die Migrationen konnten nicht geprüft werden: {ex.Message}";

                return new DatabaseRepositoryStatus
                {
                    ConfigurationFileExists = true,
                    ConfigurationIsValid = true,
                    HasConnectionString = true,
                    IsServerReachable = true,
                    IsDatabaseReachable = true,
                    HasPendingMigrations = true,
                    StatusMessage = "Die Datenbank ist erreichbar, aber der Migrationsstatus konnte nicht geprüft werden."
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
                StatusMessage = hasPendingMigrations ? "Die Datenbank ist erreichbar, aber es stehen noch Migrationen aus." : "Die Datenbank ist vollständig eingerichtet."
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

            try
            {
                await connection.ExecuteAsync(sql);
            }
            catch (SqlException ex) when (ex.Number == 1801)
            {
                // Fehler 1801: "Database already exists."
                // Das ist kein Fehler, sondern bedeutet, dass die Datenbank bereits existiert.
                // Wir ignorieren diese Ausnahme einfach.
            }
        }
        private async Task<bool> TestConnectionStringAsync(string connectionString)
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
        private static DatabaseRepositoryStatus CreateInvalidConfigurationStatus(string message)
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