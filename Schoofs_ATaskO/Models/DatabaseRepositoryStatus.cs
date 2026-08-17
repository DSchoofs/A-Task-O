namespace Schoofs_ATaskO.Models
{
    public class DatabaseRepositoryStatus
    {
        public bool HasConnectionString { get; init; }

        public bool IsServerReachable { get; init; }

        public bool IsDatabaseReachable { get; init; }

        public bool HasPendingMigrations { get; init; }
    }
}