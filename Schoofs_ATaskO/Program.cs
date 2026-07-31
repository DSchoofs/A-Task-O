using Schoofs_ATaskO.Repositories;

namespace Schoofs_ATaskO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Razor Pages registrieren (macht es erst zu einem Razor Projekt)
            builder.Services.AddRazorPages();

            // Repos registrieren, damit sie in den Razor Pages verwendet werden können
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITaskRepository, TaskRepository>();
            builder.Services.AddScoped<IEntryRepository, EntryRepository>();

            var app = builder.Build();

            app.UseRouting();

            // nutzt den Ordner wwwroot in dem z.B. das css liegt oder images etc
            app.UseStaticFiles();


            // Razor Pages aktivieren
            app.MapRazorPages();

            app.Run();
        }
    }
}
