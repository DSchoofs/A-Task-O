namespace Schoofs_ATaskO.Services
{
    public interface IDatabaseConfigurationStorage
    {
        Task SaveConnectionStringAsync(string connectionString);
    }
}