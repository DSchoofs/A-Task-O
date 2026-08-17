using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Schoofs_ATaskO.ViewModels;
using Schoofs_ATaskO.Models;

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
                    await CreateDatabaseAsync( connection, DatabaseName);
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

            var connectionString =
                _configuration.GetConnectionString("ATaskO");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return new DatabaseRepositoryStatus
                {
                    HasConnectionString = false,
                    IsServerReachable = false,
                    IsDatabaseReachable = false,
                    HasPendingMigrations = true
                };
            }

            var isDatabaseReachable =
                await TestStoredDatabaseConnectionAsync(
                    connectionString);

            if (!isDatabaseReachable)
            {
                var serverConnectionString =
                    RemoveDatabaseName(connectionString);

                var isServerReachable =
                    await TestServerConnectionAsync(
                        serverConnectionString);

                return new DatabaseRepositoryStatus
                {
                    HasConnectionString = true,
                    IsServerReachable = isServerReachable,
                    IsDatabaseReachable = false,
                    HasPendingMigrations = true
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
            }

            var status = new DatabaseRepositoryStatus
            {
                HasConnectionString = true,
                IsServerReachable = true,
                IsDatabaseReachable = true,
                HasPendingMigrations = hasPendingMigrations
            };

            return status;
        }

        private async Task<bool> DoesDatabaseExistAsync(SqlConnection connection, string databaseName)
        {
            const string sql = "SELECT COUNT(1) FROM sys.databases WHERE name = @DatabaseName";

            var databaseCount = await connection.ExecuteScalarAsync<int>(sql, new { DatabaseName = databaseName });

            return databaseCount > 0;
        }

        private async Task CreateDatabaseAsync( SqlConnection connection, string databaseName)
        {
            var escapedDatabaseName = databaseName.Replace("]", "]]");

            var sql = $"CREATE DATABASE [{escapedDatabaseName}]";

            await connection.ExecuteAsync(sql);
        }

        private async Task<bool> TestStoredDatabaseConnectionAsync( string connectionString)
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

        private string BuildServerConnectionString( DatabaseConfigurationViewModel model)
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
            var builder =new SqlConnectionStringBuilder(BuildServerConnectionString(model))
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
    }
}