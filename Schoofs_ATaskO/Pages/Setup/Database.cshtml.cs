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
        private readonly ILogger<DatabaseModel> _logger;

        [BindProperty]
        public DatabaseConfigurationViewModel Configuration { get; set; } = new();

        public DatabaseSetupStatus Status { get; private set; } = new();

        public string? Message { get; private set; }

        public bool? IsSuccess { get; private set; }

        public bool ConnectionTestSuccessful { get; private set; }

        public bool ConfigurationSaved { get; private set; }

        public bool RequiresRestart { get; private set; }

        public DatabaseModel(IDatabaseSetupService databaseSetupService, ILogger<DatabaseModel> logger)
        {
            _databaseSetupService = databaseSetupService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Status = await _databaseSetupService.GetStatusAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostTestConnectionAsync()
        {
            var testSuccessful = await _databaseSetupService.TestServerConnectionAsync(Configuration);

            Status = await _databaseSetupService.GetStatusAsync();

            if (!testSuccessful)
            {
                Message = _databaseSetupService.ErrorMessage;

                IsSuccess = false;

                return Page();
            }

            Message = "Der SQL-Server ist erreichbar. Die Datenbank ATaskO wurde damit noch nicht eingerichtet. Speichern Sie die Konfiguration, um ATaskO zu erstellen.";

            IsSuccess = true;
            ConnectionTestSuccessful = true;

            return Page();
        }

        public async Task<IActionResult> OnPostSaveConfigurationAsync()
        {
            var configurationSaved =
                await _databaseSetupService
                    .SaveConfigurationAsync(Configuration);

            if (!configurationSaved)
            {
                Status =
                    await _databaseSetupService.GetStatusAsync();

                Message =
                    _databaseSetupService.ErrorMessage;

                IsSuccess = false;

                return Page();
            }

            _logger.LogInformation(
                "Database configuration was saved successfully.");

            Message =
                "Die Datenbank wurde erfolgreich vorbereitet und die " +
                "Konfiguration gespeichert. Bitte starten Sie die Anwendung " +
                "neu, damit die neue Verbindung verwendet wird.";

            IsSuccess = true;
            ConnectionTestSuccessful = true;
            ConfigurationSaved = true;
            RequiresRestart = true;

            return Page();
        }

        public async Task<IActionResult> OnPostMigrateAsync()
        {
            Status =
                await _databaseSetupService.GetStatusAsync();

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
                Message =
                    "Es sind keine ausstehenden Migrationen vorhanden.";

                IsSuccess = true;

                return Page();
            }

            try
            {
                var migrationSuccessful =
                    await _databaseSetupService
                        .ApplyMigrationsAsync(Configuration);

                if (!migrationSuccessful)
                {
                    Message =
                        _databaseSetupService.ErrorMessage;

                    IsSuccess = false;

                    return Page();
                }

                _logger.LogInformation(
                    "Database migration completed successfully.");

                Message =
                    "Die Datenbank wurde erfolgreich eingerichtet.";

                IsSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Database migration failed.");

                Message =
                    $"Fehler beim Einrichten der Datenbank: {ex.Message}";

                IsSuccess = false;
            }

            Status =
                await _databaseSetupService.GetStatusAsync();

            return Page();
        }
    }
}