using Schoofs_ATaskO.Services;

namespace Schoofs_ATaskO.Middleware
{
    public class SetupMiddleware
    {
        private const string PepperWasStoredInSessionKey =
            "PepperWasStoredInSession";

        private readonly RequestDelegate _next;

        public SetupMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var setupStatus = context.RequestServices.GetRequiredService<ISetupStatusService>();

            if (!setupStatus.IsSetupComplete)
            {
                var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

                var setupService = context.RequestServices.GetRequiredService<ISetupService>();

                var pepperWasStoredInSession =
                    context.Session.GetString(PepperWasStoredInSessionKey) == "true";

                if (!setupService.HasPasswordPepper() &&
                    !pepperWasStoredInSession)
                {
                    if (!path.StartsWith("/setup/pepper"))
                    {
                        context.Response.Redirect("/Setup/Pepper");
                        return;
                    }

                    await _next(context);
                    return;
                }

                if (!setupService.HasPasswordPepper() &&
                    pepperWasStoredInSession)
                {
                    var allowedPaths = new[]
                    {
                        "/setup/pepper",
                        "/setup/database",
                        "/css",
                        "/js",
                        "/lib",
                        "/favicon.ico",
                        "/account/accessdenied"
                    };

                    var isAllowedPath =
                        allowedPaths.Any(allowedPath => path.StartsWith(allowedPath));

                    if (!isAllowedPath)
                    {
                        context.Response.Redirect("/Setup/Database");
                        return;
                    }

                    await _next(context);
                    return;
                }

                var isDatabaseReady = await setupService.IsDatabaseReadyAsync();

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

                if (!setupService.HasAdminUser())
                {
                    if (!path.StartsWith("/setup/initialadmin"))
                    {
                        context.Response.Redirect("/Setup/InitialAdmin");
                        return;
                    }

                    await _next(context);
                    return;
                }
            }

            await _next(context);
        }
    }
}