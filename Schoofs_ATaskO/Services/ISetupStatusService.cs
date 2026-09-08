namespace Schoofs_ATaskO.Services
{
    public interface ISetupStatusService
    {
        bool IsSetupComplete { get; }
        void Initialize();
        void Refresh();
    }
}