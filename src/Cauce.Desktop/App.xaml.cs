using System;
using System.Windows;
using Cauce.Desktop.Themes;
using Cauce.Desktop.ViewModels;
using Cauce.Desktop.Infrastructure;

namespace Cauce.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeManager.Apply("Sistema", false);
        var viewModel = new MainViewModel();
        var window = new MainWindow { DataContext = viewModel };
        MainWindow = window;
        var tray = new TrayService(window, viewModel);
        window.Closed += (_, _) => tray.Dispose();
        window.Loaded += async (_, _) => await viewModel.InitializeAsync();
        window.Show();
    }
}
