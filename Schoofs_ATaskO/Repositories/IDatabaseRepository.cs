using Schoofs_ATaskO.ViewModels;
using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Repositories
{
    public interface IDatabaseRepository
    {
        Task<bool> TestServerConnectionAsync(DatabaseConfigurationViewModel model);
        Task<bool> EnsureDatabaseExistsAsync(DatabaseConfigurationViewModel model);
        Task<bool> TestDatabaseConnectionAsync(DatabaseConfigurationViewModel model);
        Task<bool> ApplyMigrationsAsync(DatabaseConfigurationViewModel model);
        Task<DatabaseRepositoryStatus> GetStatusAsync();
        public string ErrorMessage { get; set; }
    }

    
}