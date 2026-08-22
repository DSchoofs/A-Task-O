using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services
{
    public interface ISetupService
    {
        bool HasPasswordPepper();
        Task<bool> IsDatabaseReadyAsync();
        bool HasAdminUser();
        string GeneratePasswordPepper();
        bool CreatePasswordPepper(string pepper);
        bool IsInitialSetupRequired();
        bool CreateInitialAdmin(InitialAdminViewModel model);

        string ErrorMessage { get; }
    }
}