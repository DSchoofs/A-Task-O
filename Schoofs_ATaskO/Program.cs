using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Schoofs_ATaskO.Repositories;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.Settings;

namespace Schoofs_ATaskO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {

                var builder = WebApplication.CreateBuilder(args);

                //Razor Pages registrieren (macht es erst zu einem Razor Projekt)
                builder.Services.AddRazorPages();

                builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ATaskO") ?? throw new InvalidOperationException("Connection string 'ATaskO' not found.")));

                var securityFilePath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "security.json");

                builder.Configuration.AddJsonFile(securityFilePath, optional: true, reloadOnChange: false);

                builder.Services.AddOptions<SecuritySettings>().Bind(builder.Configuration.GetSection(SecuritySettings.SectionName));

                // Basis für die Authentifizierung und Autorisierung registrieren, damit man sich einloggen kann
                builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

                builder.Services.AddAuthorization();
                builder.Services.AddHttpContextAccessor();
                // Repos registrieren, damit sie in den Razor Pages verwendet werden können
                builder.Services.AddScoped<ISetupService, SetupService>();
                builder.Services.AddScoped<IUserRepository, UserRepository>();
                builder.Services.AddScoped<ITaskRepository, TaskRepository>();
                builder.Services.AddScoped<IEntryRepository, EntryRepository>();
                builder.Services.AddScoped<IPasswordService, PasswordService>();
                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<ITaskService, TaskService>();
                builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
                builder.Services.AddScoped<ITaskBlockRepository, TaskBlockRepository>();
                builder.Services.AddScoped<ITaskBlockService, TaskBlockService>();

                var app = builder.Build();

                app.UseRouting();

                // nutzt den Ordner wwwroot in dem z.B. das css liegt oder images etc
                app.UseStaticFiles();

                // Authentifizierung und Autorisierung aktivieren
                app.UseAuthentication();
                app.UseAuthorization();

                app.Use(async (context, next) =>
                {
                    var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

                    var allowedPaths = new[]
                    {
                    "/setup/pepper",
                    "/setup/initialadmin",
                    "/css",
                    "/js",
                    "/lib",
                    "/favicon.ico",
                    "/account/accessdenied"
                };

                    var isAllowedPath = allowedPaths.Any(allowedPath => path.StartsWith(allowedPath));

                    var setupService = context.RequestServices.GetRequiredService<ISetupService>();

                    if (!setupService.HasPasswordPepper())
                    {
                        if (!path.StartsWith("/setup/pepper"))
                        {
                            context.Response.Redirect("/Setup/Pepper");
                            return;
                        }

                        await next();
                        return;
                    }

                    if (!isAllowedPath && setupService.IsInitialSetupRequired())
                    {
                        context.Response.Redirect("/Setup/InitialAdmin");
                        return;
                    }

                    await next();
                });

                // Razor Pages aktivieren
                app.MapRazorPages();

                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}