using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AppServices;

public partial class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : DbContext(options)
{
    public DbSet<Stage> Stages => Set<Stage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Performance> Performances => Set<Performance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stage>()
            .Property(e => e.Name)
            .IsRequired();

        modelBuilder.Entity<Category>()
            .Property(e => e.Name)
            .IsRequired();

        modelBuilder.Entity<Artist>()
            .Property(e => e.Name)
            .IsRequired();

        modelBuilder.Entity<Artist>()
            .HasMany(e => e.Categories)
            .WithMany(e => e.Artists);

        modelBuilder.Entity<Category>()
            .HasOne(e => e.Winner)
            .WithMany(e => e.WinningCategories)
            .HasForeignKey(e => e.WinnerId);
    }
}

public class ApplicationDataContextFactory : IDesignTimeDbContextFactory<ApplicationDataContext>
{
    public ApplicationDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDataContext>();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var path = configuration["Database:path"] ?? throw new InvalidOperationException("Database path not configured.");
        var fileName = configuration["Database:fileName"] ?? throw new InvalidOperationException("Database file name not configured.");
        optionsBuilder.UseSqlite($"Data Source={path}/{fileName}");

        return new ApplicationDataContext(optionsBuilder.Options);
    }
}