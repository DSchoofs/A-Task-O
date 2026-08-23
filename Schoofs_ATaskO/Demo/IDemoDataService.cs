namespace Schoofs_ATaskO.Demo
{
    public interface IDemoDataService
    {
        bool CanCreateDemoData();
        bool CreateDemoData();
        string ErrorMessage { get; }
    }
}