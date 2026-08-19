using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Repositories
{
    public interface IDatabaseRepository
    {
        Task<bool> TestServerConnectionAsync(DatabaseConfigurationViewModel model);

        Task<bool> EnsureDatabaseExistsAsync(DatabaseConfigurationViewModel model);

        Task<bool> TestDatabaseConnectionAsync(DatabaseConfigurationViewModel model);

        Task<bool> ApplyMigrationsAsync(DatabaseConfigurationViewModel model);

        Task<DatabaseRepositoryStatus> GetStatusAsync();

        string BuildDatabaseConnectionString(DatabaseConfigurationViewModel model);

        string ErrorMessage { get; set; }
    }
}