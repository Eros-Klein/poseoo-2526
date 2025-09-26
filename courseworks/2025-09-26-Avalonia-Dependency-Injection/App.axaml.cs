using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using Avalonia.Markup.Xaml;
using _2025_09_26_Avalonia_Dependency_Injection.ViewModels;
using _2025_09_26_Avalonia_Dependency_Injection.Views;
using Microsoft.Extensions.DependencyInjection;

namespace _2025_09_26_Avalonia_Dependency_Injection;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var collection = new ServiceCollection();
            collection.AddTransient<MainWindowViewModel>();
            collection.AddTransient<MainWindow>();
            collection.AddTransient<ToDoListView>();
            collection.AddTransient<ToDoListViewModel>();

            var services = collection.BuildServiceProvider();
            
            desktop.MainWindow = services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}