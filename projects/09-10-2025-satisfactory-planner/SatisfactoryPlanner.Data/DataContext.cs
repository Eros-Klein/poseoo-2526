using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SatisfactoryPlanner.Data;

public partial class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options)
    : DbContext(options)
{
    public DbSet<Machine> Machines =>  Set<Machine>();
    public DbSet<Recipe> Recipes =>  Set<Recipe>();
    public DbSet<Element> Elements =>  Set<Element>();
    public DbSet<ElementLine> ElementLines =>  Set<ElementLine>();
}

public class ApplicationDataContextFactory : IDesignTimeDbContextFactory<ApplicationDataContext>
{
    public ApplicationDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDataContext>();

        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        var configuration = configurationBuilder.Build();
            
        optionsBuilder.UseSqlite(configuration.GetConnectionString("SatisfactoryPlanner"));
        return new ApplicationDataContext(optionsBuilder.Options);
    }
}