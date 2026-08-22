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

            var builder = WebApplication.CreateBuilder(args);

            var databaseFilePath = Path.Combine(builder.Environment.ContentRootPath,"App_Data","database.json");

            builder.Configuration.AddJsonFile(databaseFilePath, optional: true, reloadOnChange: false);
            
            builder.Services.AddRazorPages();

            var configuredConnectionString = builder.Configuration.GetConnectionString("ATaskO");

            var bootstrapConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True;";

            var applicationConnectionString = string.IsNullOrWhiteSpace(configuredConnectionString) ? bootstrapConnectionString : configuredConnectionString;

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(applicationConnectionString));

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
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<ITaskBlockRepository, TaskBlockRepository>();
            builder.Services.AddScoped<ITaskBlockService, TaskBlockService>();
            builder.Services.AddScoped<IDatabaseSetupService, DatabaseSetupService>();
            builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();
            builder.Services.AddScoped<IDatabaseConfigurationStorage, DatabaseConfigurationStorage>();

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
                    "/setup/database",
                    "/setup/initialadmin",
                    "/css",
                    "/js",
                    "/lib",
                    "/favicon.ico",
                    "/account/accessdenied"
                };

                var isAllowedPath = allowedPaths.Any(allowedPath => path.StartsWith(allowedPath));

                var setupService = context.RequestServices.GetRequiredService<ISetupService>();
                var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
                var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

                // 1. Pepper prüfen
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

                // 2. Datenbank prüfen
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

                    await next();
                    return;
                }

                // 3. Admin prüfen (nur wenn Datenbank bereit ist)
                var adminExists = await dbContext.Users.AnyAsync(u => u.IsAdmin);

                if (!adminExists)
                {
                    if (!path.StartsWith("/setup/initialadmin"))
                    {
                        context.Response.Redirect("/Setup/InitialAdmin");
                        return;
                    }

                    await next();
                    return;
                }

                await next();
            });

            // Razor Pages aktivieren
            app.MapRazorPages();

            app.Run();

        }
    }
}