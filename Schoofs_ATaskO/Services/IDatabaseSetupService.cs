using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services
{
    public interface IDatabaseSetupService
    {
        Task<DatabaseSetupStatus> GetStatusAsync();

        Task<bool> TestLocalSqlServerAsync();

        Task<bool> TestServerConnectionAsync(DatabaseConfigurationViewModel model);

        Task<bool> SaveConfigurationAsync(DatabaseConfigurationViewModel model);

        Task<bool> ApplyMigrationsAsync(DatabaseConfigurationViewModel model);

        string ErrorMessage { get; }
    }
}