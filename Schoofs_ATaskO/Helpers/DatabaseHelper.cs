using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Helpers
{
    public static class DatabaseHelper
    {
        private const string DatabaseName = "ATaskO";
        private const int MaxRetryAttempts = 60;
        private const int RetryDelayMilliseconds = 1000;

        public static string BuildServerConnectionString(DatabaseConfigurationViewModel model)
        {
            if (model.UseDefaultConfiguration)
            {
                return @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True;";
            }

            if (string.IsNullOrWhiteSpace(model.ServerName))
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

        public static string BuildDatabaseConnectionString(DatabaseConfigurationViewModel model)
        {
            var builder = new SqlConnectionStringBuilder(BuildServerConnectionString(model))
            {
                InitialCatalog = DatabaseName
            };

            return builder.ConnectionString;
        }

        public static string RemoveDatabaseName(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            builder.Remove("Initial Catalog");
            builder.Remove("Database");

            return builder.ConnectionString;
        }

        public static async Task<bool> WaitForDatabaseConnectionAsync(string connectionString)
        {
            for (var attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    await using var connection = new SqlConnection(connectionString);

                    await connection.OpenAsync();

                    Console.WriteLine("Die Datenbank ist erreichbar.");

                    return true;
                }
                catch
                {
                    if (attempt == 1 || attempt % 5 == 0)
                    {
                        Console.WriteLine($"Die Datenbank wird vorbereitet. Verbindungsversuch {attempt} von {MaxRetryAttempts} ...");
                    }
                }

                if (attempt < MaxRetryAttempts)
                {
                    await Task.Delay(RetryDelayMilliseconds);
                }
            }

            Console.WriteLine("Die Datenbank konnte innerhalb der Wartezeit nicht erreicht werden.");

            return false;
        }

        public static async Task<bool> WaitForMigrationsAsync(string connectionString)
        {
            for (var attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    await using var dbContext = CreateDbContext(connectionString);

                    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

                    if (!pendingMigrations.Any())
                    {
                        Console.WriteLine("Die Datenbankmigrationen sind abgeschlossen.");

                        return true;
                    }
                }
                catch
                {
                    if (attempt == 1 || attempt % 5 == 0)
                    {
                        Console.WriteLine( $"Der Migrationsstatus wird geprüft. Versuch {attempt} von {MaxRetryAttempts} ...");
                    }
                }

                if (attempt < MaxRetryAttempts)
                {
                    await Task.Delay(RetryDelayMilliseconds);
                }
            }

            Console.WriteLine( "Die Migrationen konnten innerhalb der Wartezeit nicht bestätigt werden.");

            return false;
        }

        public static async Task<bool> ApplyMigrationsWithRetryAsync(string connectionString)
        {
            for (var attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    if (attempt == 1)
                    {
                        Console.WriteLine("Datenbankmigrationen werden angewendet ...");
                    }

                    await using var dbContext = CreateDbContext(connectionString);

                    await dbContext.Database.MigrateAsync();

                    Console.WriteLine("Datenbankmigrationen wurden angewendet.");

                    return true;
                }
                catch (Exception) when (attempt < MaxRetryAttempts)
                {
                    if (attempt == 1 || attempt % 5 == 0)
                    {
                        Console.WriteLine( $"Die Migrationen warten auf die Datenbank. Versuch {attempt} von {MaxRetryAttempts} ...");
                    }

                    await Task.Delay(RetryDelayMilliseconds);
                }
                catch
                {
                    throw;
                }
            }

            return false;
        }

        private static ApplicationDbContext CreateDbContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private static void ConfigureAuthentication(SqlConnectionStringBuilder builder,DatabaseConfigurationViewModel model)
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
    }
}