using Microsoft.EntityFrameworkCore;
using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Middleware
{
    public class SetupMiddleware
    {
        private readonly RequestDelegate _next;

        public SetupMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            var setupService = context.RequestServices.GetRequiredService<ISetupService>();
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
            var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

            if (!setupService.HasPasswordPepper())
            {
                if (!path.StartsWith("/setup/pepper"))
                {
                    context.Response.Redirect("/Setup/Pepper");
                    return;
                }

                await _next(context);
                return;
            }

            var connectionString = configuration.GetConnectionString("ATaskO");
            var isDatabaseReady = false;

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                try
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                    optionsBuilder.UseSqlServer(connectionString);

                    await using var checkContext = new ApplicationDbContext(optionsBuilder.Options);

                    var canConnect = await checkContext.Database.CanConnectAsync();

                    if (canConnect)
                    {
                        var pendingMigrations = await checkContext.Database.GetPendingMigrationsAsync();

                        isDatabaseReady = !pendingMigrations.Any();
                    }
                }
                catch
                {
                    isDatabaseReady = false;
                }
            }

            if (!isDatabaseReady)
            {
                if (!path.StartsWith("/setup/database"))
                {
                    context.Response.Redirect("/Setup/Database");
                    return;
                }

                await _next(context);
                return;
            }

            var adminExists = await dbContext.Users.AnyAsync(u => u.IsAdmin);

            if (!adminExists)
            {
                if (!path.StartsWith("/setup/initialadmin"))
                {
                    context.Response.Redirect("/Setup/InitialAdmin");
                    return;
                }

                await _next(context);
                return;
            }

            await _next(context);
        }
    }
}