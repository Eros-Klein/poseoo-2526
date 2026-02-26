using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using AppServices;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Importer;

internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var parser = new CommandLineParser();
            var result = parser.Parse(args);
            var filePath = result.CsvFilePath;

            // TODO: Use result.IsDryRun when Importer supports it

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    // Reflection to find the Importer class in AppServices
                    var appServicesAssembly = typeof(AppServices.ApplicationDataContext).Assembly;
                    var importerType = appServicesAssembly.GetType("AppServices.Importer.Importer");

                    if (importerType == null)
                    {
                        Console.WriteLine("AppServices.Importer.Importer class not found. Please implement it in the AppServices project.");
                        return;
                    }

                    // Create an instance of the Importer class using DI
                    var importerInstance = ActivatorUtilities.CreateInstance(services, importerType);

                    // Find the ImportAsync method
                    var importMethod = importerType.GetMethod("ImportAsync", [typeof(string)]);
                    if (importMethod == null)
                    {
                        Console.WriteLine("ImportAsync method not found in AppServices.Importer.Importer.");
                        return;
                    }

                    // Invoke the method
                    if (importMethod.Invoke(importerInstance, [filePath]) is Task task)
                    {
                        await task;
                        Console.WriteLine("Import completed successfully.");
                    }
                    else
                    {
                        Console.WriteLine("ImportAsync did not return a Task.");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during import: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var path = configuration["Database:path"] ?? throw new InvalidOperationException("Database path not configured.");
                var fileName = configuration["Database:fileName"] ?? throw new InvalidOperationException("Database file name not configured.");

                services.AddDbContext<ApplicationDataContext>(options =>
                    options.UseSqlite($"Data Source={path}/{fileName}"));

                services.AddScoped<AppServices.Importer.IFileReader, AppServices.Importer.FileReader>();
            });
}
