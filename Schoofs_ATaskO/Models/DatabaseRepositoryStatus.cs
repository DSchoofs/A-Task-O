namespace Schoofs_ATaskO.Models
{
    public class DatabaseRepositoryStatus
    {
        public bool ConfigurationFileExists { get; init; }
        public bool ConfigurationIsValid { get; init; }
        public bool HasConnectionString { get; init; }
        public bool IsServerReachable { get; init; }
        public bool IsDatabaseReachable { get; init; }
        public bool HasPendingMigrations { get; init; }
        public string StatusMessage { get; init; } = string.Empty;
        public bool IsFullyReady => ConfigurationFileExists && ConfigurationIsValid &&  HasConnectionString && 
                                    IsServerReachable && IsDatabaseReachable && !HasPendingMigrations;
    }
}