namespace Schoofs_ATaskO.Services
{
    public class SetupStatusService : ISetupStatusService
    {
        private readonly IServiceProvider _services;

        public bool IsSetupComplete { get; private set; }

        public SetupStatusService(IServiceProvider services)
        {
            _services = services;
        }

        public void Initialize()
        {
            using var scope = _services.CreateScope();

            var setupService = scope.ServiceProvider.GetRequiredService<ISetupService>();

            var hasPepper = setupService.HasPasswordPepper();

            var isDbReady = setupService.IsDatabaseReadyAsync().GetAwaiter().GetResult();

            var hasAdmin = setupService.HasAdminUser();

            IsSetupComplete = hasPepper && isDbReady && hasAdmin;
        }
    }
}