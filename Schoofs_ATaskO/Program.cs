using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Schoofs_ATaskO.Demo;
using Schoofs_ATaskO.Middleware;
using Schoofs_ATaskO.Repositories;
using Schoofs_ATaskO.Services;
using Schoofs_ATaskO.Settings;

namespace Schoofs_ATaskO
{
    public class Program
    {
        public static async Task Main(string[] args)
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
            builder.Services.AddScoped<IDemoDataService, DemoDataService>();
            builder.Services.AddSingleton<ISetupStatusService, SetupStatusService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();

            var app = builder.Build();
            var setupStatus = app.Services.GetRequiredService<ISetupStatusService>();
            setupStatus.Initialize();

            app.UseRouting();

            // nutzt den Ordner wwwroot in dem z.B. das css liegt oder images etc
            app.UseStaticFiles();

            // Authentifizierung und Autorisierung aktivieren
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<SetupMiddleware>();

            // Razor Pages aktivieren
            app.MapRazorPages();

            await app.RunAsync();

        }
    }
}