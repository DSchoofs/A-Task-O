using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
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
            Console.WriteLine("A-Task-O wird gestartet. Das kann einen kurzen Moment dauern ...");

            var builder = WebApplication.CreateBuilder(args);

            var databaseFilePath = Path.Combine(builder.Environment.ContentRootPath,"App_Data","database.json");

            builder.Configuration.AddJsonFile(databaseFilePath,optional: true,reloadOnChange: false);

            builder.Services.AddRazorPages();

            var configuredConnectionString = builder.Configuration.GetConnectionString("ATaskO");

            var bootstrapConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;TrustServerCertificate=True;";

            var applicationConnectionString = string.IsNullOrWhiteSpace(configuredConnectionString) ? bootstrapConnectionString : configuredConnectionString;

            builder.Services.AddDbContext<ApplicationDbContext>( options => options.UseSqlServer(applicationConnectionString));

            var securityFilePath = Path.Combine(builder.Environment.ContentRootPath,"App_Data","security.json");

            builder.Configuration.AddJsonFile( securityFilePath, optional: true, reloadOnChange: false);

            builder.Services.AddOptions<SecuritySettings>().Bind(builder.Configuration.GetSection(SecuritySettings.SectionName));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();

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

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<SetupMiddleware>();

            app.MapRazorPages();

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                var server = app.Services.GetRequiredService<IServer>();

                var addressesFeature = server.Features.Get<IServerAddressesFeature>();

                var addresses = addressesFeature?.Addresses.OrderBy(address => address).ToList() ?? new List<string>();

                Console.WriteLine();
                Console.WriteLine("A-Task-O ist bereit.");

                if (addresses.Count > 0)
                {
                    Console.WriteLine("Bitte öffnen Sie im Browser:");

                    foreach (var address in addresses)
                    {
                        Console.WriteLine(address);
                    }
                }
                else
                {
                    Console.WriteLine("Bitte öffnen Sie die konfigurierte Anwendungsadresse im Browser.");
                }

                Console.WriteLine();
                Console.WriteLine("Zum Beenden der Anwendung drücken Sie im Konsolenfenster Strg + C.");
                Console.WriteLine();
            });

            app.Lifetime.ApplicationStopping.Register(() =>
            {
                Console.WriteLine();
                Console.WriteLine("A-Task-O wird beendet.");
                Console.WriteLine("Sie können dieses Fenster jetzt schließen.");
            });

            await app.RunAsync();
        }
    }
}