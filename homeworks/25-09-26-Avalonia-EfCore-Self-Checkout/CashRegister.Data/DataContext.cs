using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CashRegister.Data;

public partial class ApplicationDataContext(DbContextOptions<ApplicationDataContext> options)
    : DbContext(options)
{
    public DbSet<Greeting> Greetings => Set<Greeting>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<ReceiptLine> ReceiptLines => Set<ReceiptLine>();
}

public class ApplicationDataContextFactory : IDesignTimeDbContextFactory<ApplicationDataContext>
{
    public ApplicationDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDataContext>();

        var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            var configuration = configurationBuilder.Build();
            
        optionsBuilder.UseSqlite(configuration.GetConnectionString("CashRegister"));
        return new ApplicationDataContext(optionsBuilder.Options);
    }
}