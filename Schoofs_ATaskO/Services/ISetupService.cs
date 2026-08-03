using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services
{
    public interface ISetupService
    {
        bool HasAdminUser();
        bool IsInitialSetupRequired();
        bool CreateInitialAdmin(InitialAdminViewModel model);
        string ErrorMessage { get; }
    }
}