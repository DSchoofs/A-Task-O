using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Helpers
{
    public static class DatabaseHelper
    {
        private const string DatabaseName = "ATaskO";

        public static string BuildServerConnectionString(
            DatabaseConfigurationViewModel model)
        {
            if (model.UseDefaultConfiguration)
            {
                return
                    @"Data Source=(localdb)\MSSQLLocalDB;" +
                    "Integrated Security=True;" +
                    "TrustServerCertificate=True;";
            }

            if (string.IsNullOrWhiteSpace(model.ServerName))
            {
                throw new InvalidOperationException(
                    "Bitte einen Server oder eine Instanz angeben.");
            }

            var builder =
                new SqlConnectionStringBuilder
                {
                    DataSource = model.ServerName,
                    TrustServerCertificate = true
                };

            ConfigureAuthentication(builder, model);

            return builder.ConnectionString;
        }

        public static string BuildDatabaseConnectionString(
            DatabaseConfigurationViewModel model)
        {
            var builder =
                new SqlConnectionStringBuilder(
                    BuildServerConnectionString(model))
                {
                    InitialCatalog = DatabaseName
                };

            return builder.ConnectionString;
        }

        public static string RemoveDatabaseName(
            string connectionString)
        {
            var builder =
                new SqlConnectionStringBuilder(
                    connectionString);

            builder.Remove("Initial Catalog");
            builder.Remove("Database");

            return builder.ConnectionString;
        }

        public static async Task<bool> WaitForDatabaseConnectionAsync(
            string connectionString)
        {
            const int maxAttempts = 20;
            const int delayMilliseconds = 500;

            for (var attempt = 1;
                 attempt <= maxAttempts;
                 attempt++)
            {
                try
                {
                    await using var connection =
                        new SqlConnection(connectionString);

                    await connection.OpenAsync();

                    return true;
                }
                catch 
                {
                    
                }

                if (attempt < maxAttempts)
                {
                    await Task.Delay(
                        delayMilliseconds);
                }
            }

            return false;
        }

        public static async Task<bool> WaitForMigrationsAsync(
            string connectionString)
        {
            const int maxAttempts = 20;
            const int delayMilliseconds = 500;

            for (var attempt = 1;
                 attempt <= maxAttempts;
                 attempt++)
            {
                try
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

                    optionsBuilder.UseSqlServer(connectionString);

                    await using var dbContext = new ApplicationDbContext(optionsBuilder.Options);

                    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

                    if (!pendingMigrations.Any())
                    {
                        return true;
                    }
                }
                catch 
                {
                    
                }

                if (attempt < maxAttempts)
                {
                    await Task.Delay(delayMilliseconds);
                }
            }

            return false;
        }
        public static async Task<bool> ApplyMigrationsWithRetryAsync(
    string connectionString)
        {
            const int maxAttempts = 10;
            const int delayMilliseconds = 1000;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                    optionsBuilder.UseSqlServer(connectionString);

                    await using var dbContext = new ApplicationDbContext(optionsBuilder.Options);

                    await dbContext.Database.MigrateAsync();

                    return true;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    // Bei Fehlern vor dem letzten Versuch: Warte kurz und versuche es erneut.
                    await Task.Delay(delayMilliseconds);
                }
                catch
                {
                    // Beim letzten Versuch: Exception weiterwerfen.
                    throw;
                }
            }

            return false;
        }
        private static void ConfigureAuthentication(
            SqlConnectionStringBuilder builder,
            DatabaseConfigurationViewModel model)
        {
            if (model.AuthenticationMode == "SqlServer")
            {
                if (string.IsNullOrWhiteSpace(model.Username) ||
                    string.IsNullOrWhiteSpace(model.Password))
                {
                    throw new InvalidOperationException(
                        "Für die SQL-Server-Authentifizierung werden " +
                        "Benutzername und Passwort benötigt.");
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
    }
}