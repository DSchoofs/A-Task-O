using Microsoft.AspNetCore.Authentication.Cookies;
using Schoofs_ATaskO.Repositories;
using Schoofs_ATaskO.Settings;

namespace Schoofs_ATaskO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Razor Pages registrieren (macht es erst zu einem Razor Projekt)
            builder.Services.AddRazorPages();

            builder.Services
                .AddOptions<SecuritySettings>()
                .Bind(builder.Configuration.GetSection(SecuritySettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Basis für die Authentifizierung und Autorisierung registrieren, damit man sich einloggen kann
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });
            builder.Services.AddAuthorization();

            // Repos registrieren, damit sie in den Razor Pages verwendet werden können
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITaskRepository, TaskRepository>();
            builder.Services.AddScoped<IEntryRepository, EntryRepository>();

            var app = builder.Build();

            app.UseRouting();

            // nutzt den Ordner wwwroot in dem z.B. das css liegt oder images etc
            app.UseStaticFiles();

            // Authentifizierung und Autorisierung aktivieren
            app.UseAuthentication();
            app.UseAuthorization();

            // Razor Pages aktivieren
            app.MapRazorPages();

            app.Run();
        }
    }
}
