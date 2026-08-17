namespace Schoofs_ATaskO.Models
{
    public class DatabaseSetupStatus
    {
        public bool HasConnectionString { get; init; }

        public bool IsServerReachable { get; init; }

        public bool IsDatabaseReachable { get; init; }

        public bool HasPendingMigrations { get; init; }

        public bool IsFullyReady => HasConnectionString && IsServerReachable && IsDatabaseReachable && !HasPendingMigrations;
    }
}