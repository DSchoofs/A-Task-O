using Microsoft.EntityFrameworkCore;
using Schoofs_ATaskO.Models;
using Schoofs_ATaskO.Configuration;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Entitäten(ModelKlassen) <-> Tabellen
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<TaskBlock> TaskBlocks { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // EF-Konfigurationen anhängen
        modelBuilder.ApplyConfiguration(new TaskItemConfiguration());
        modelBuilder.ApplyConfiguration(new TaskBlockConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}