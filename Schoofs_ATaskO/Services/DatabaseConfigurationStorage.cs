using System.Text.Json;

namespace Schoofs_ATaskO.Services
{
    public class DatabaseConfigurationStorage : IDatabaseConfigurationStorage
    {
        private readonly IWebHostEnvironment _environment;

        public DatabaseConfigurationStorage(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task SaveConnectionStringAsync(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Der Connection String darf nicht leer sein.",nameof(connectionString));
            }

            var appDataPath = Path.Combine(_environment.ContentRootPath, "App_Data");

            Directory.CreateDirectory(appDataPath);

            var databaseFilePath = Path.Combine(appDataPath, "database.json");

            var databaseConfiguration = new
            {
                ConnectionStrings = new
                {
                    ATaskO = connectionString
                }
            };

            var json = JsonSerializer.Serialize(databaseConfiguration, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(databaseFilePath, json);

            Console.WriteLine("--------------------------------------------------------------------------");
            Console.WriteLine("Die Datenbankkonfiguration wurde gespeichert.");
            Console.WriteLine("Bitte schließen Sie die Anwendung mit Strg + C und starten Sie sie erneut.");
            Console.WriteLine("--------------------------------------------------------------------------");
        }
    }
}