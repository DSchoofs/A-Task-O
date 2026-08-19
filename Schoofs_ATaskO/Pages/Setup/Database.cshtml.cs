using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Pages.Setup
{
    public class DatabaseModel : PageModel
    {
        private readonly IDatabaseSetupService _databaseSetupService;

        [BindProperty]
        public DatabaseConfigurationViewModel Configuration { get; set; } = new();
        public DatabaseSetupStatus Status { get; private set; } = new();
        public string? Message { get; private set; }
        public bool? IsSuccess { get; private set; }
        public bool ConnectionTestSuccessful { get; private set; }
        public bool RequiresRestart { get; private set; }
        public bool LocalSqlServerAvailable { get; private set; }
        
        public DatabaseModel(IDatabaseSetupService databaseSetupService, ILogger<DatabaseModel> logger)
        {
            _databaseSetupService = databaseSetupService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadPageStateAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostTestConnectionAsync()
        {
            await LoadPageStateAsync();

            var testSuccessful = await _databaseSetupService.TestServerConnectionAsync(Configuration);

            if (!testSuccessful)
            {
                Message = _databaseSetupService.ErrorMessage;
                IsSuccess = false;

                return Page();
            }

            Message ="Die ausgewählte SQL-Server-Instanz ist erreichbar. " +
                     "Sie können die Konfiguration jetzt speichern.";

            IsSuccess = true;
            ConnectionTestSuccessful = true;

            return Page();
        }

        public async Task<IActionResult> OnPostSaveConfigurationAsync()
        {
            await LoadPageStateAsync();

            var configurationSaved = await _databaseSetupService.SaveConfigurationAsync(Configuration);

            if (!configurationSaved)
            {
                Message = _databaseSetupService.ErrorMessage;
                IsSuccess = false;

                return Page();
            }


            Message = "Die Datenbankkonfiguration wurde erfolgreich gespeichert. " +
                      "Bitte beenden und starten Sie die Anwendung neu, damit die " +
                      "gespeicherte Verbindung verwendet werden kann.";

            IsSuccess = true;
            ConnectionTestSuccessful = true;
            RequiresRestart = true;

            return Page();
        }

        public async Task<IActionResult> OnPostMigrateAsync()
        {
            await LoadPageStateAsync();

            if (!Status.HasConnectionString)
            {
                Message =
                    "Es ist keine Datenbankkonfiguration vorhanden. " +
                    "Bitte konfigurieren Sie zuerst die Datenbank.";

                IsSuccess = false;

                return Page();
            }

            if (!Status.IsDatabaseReachable)
            {
                Message =
                    "Die Datenbank ist nicht erreichbar. " +
                    "Bitte testen Sie zuerst die Verbindung.";

                IsSuccess = false;

                return Page();
            }

            if (!Status.HasPendingMigrations)
            {
                Message ="Es sind keine ausstehenden Migrationen vorhanden.";

                IsSuccess = true;

                return Page();
            }

            try
            {
                var migrationSuccessful = await _databaseSetupService.ApplyMigrationsAsync(Configuration);

                if (!migrationSuccessful)
                {
                    Message = _databaseSetupService.ErrorMessage;
                    IsSuccess = false;

                    return Page();
                }


                Message ="Die Datenbank wurde erfolgreich eingerichtet.";

                IsSuccess = true;
            }
            catch (Exception ex)
            {
                Message = $"Fehler beim Einrichten der Datenbank: {ex.Message}";

                IsSuccess = false;
            }

            Status = await _databaseSetupService.GetStatusAsync();

            return Page();
        }

        private async Task LoadPageStateAsync()
        {
            LocalSqlServerAvailable = await _databaseSetupService.TestLocalSqlServerAsync();

            Status = await _databaseSetupService.GetStatusAsync();

        }
    }
}