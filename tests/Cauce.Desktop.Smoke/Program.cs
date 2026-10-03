using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cauce.Core;
using Cauce.Desktop;
using Cauce.Desktop.Themes;
using Cauce.Desktop.ViewModels;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var destination = Path.GetFullPath(args.Length == 1 ? args[0] : "artifacts/cauce-smoke");
        Directory.CreateDirectory(destination);
        var data = Path.Combine(destination, "isolated-data");
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Cauce;component/Themes/Controls.xaml") });
        app.Resources["BooleanToVisibility"] = new BooleanToVisibilityConverter();
        ThemeManager.Apply("Claro", false);
        var model = new MainViewModel(data);
        var window = new MainWindow { DataContext = model, ShowActivated = false, ShowInTaskbar = false, Left = -16000, Top = -16000 };
        app.MainWindow = window;
        var errors = new BindingErrors();
        PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
        var exitCode = 0;
        window.Loaded += async (_, _) =>
        {
            try
            {
                await model.InitializeAsync();
                await SettleAsync();
                Capture(window, Path.Combine(destination, "01-introduction.png"));
                model.FinishTutorialCommand.Execute(null);
                await SettleAsync();
                model.SelectedTheme = "Claro";
                Capture(window, Path.Combine(destination, "02-listen-light.png"));
                var tabs = Descendants(window).OfType<TabControl>().Single();
                tabs.SelectedIndex = 1;
                await SettleAsync();
                Capture(window, Path.Combine(destination, "03-library.png"));
                tabs.SelectedIndex = 2;
                foreach (var theme in model.Themes)
                {
                    model.SelectedTheme = theme;
                    await SettleAsync();
                    Capture(window, Path.Combine(destination, "theme-" + theme.Replace(' ', '-') + ".png"));
                }
                model.SelectedTheme = "Bosque";
                model.ReducedMotion = true;
                model.ReducedTransparency = true;
                if (model.MotionEnabled) throw new Exception("Reduced motion did not disable animations.");
                model.ArtistSpacing = 0;
                model.AllowRepeats = true;
                tabs.SelectedIndex = 3;
                await SettleAsync();
                Capture(window, Path.Combine(destination, "04-guide.png"));
                tabs.SelectedIndex = 4;
                await SettleAsync();
                Capture(window, Path.Combine(destination, "05-account.png"));
                if (model.CanSignIn) throw new Exception("Unconfigured test app unexpectedly enables sign-in.");
                tabs.SelectedIndex = 5;
                await SettleAsync();
                Capture(window, Path.Combine(destination, "06-support.png"));
                model.ToggleCompactCommand.Execute(null);
                await SettleAsync();
                Capture(window, Path.Combine(destination, "07-compact.png"));
                for (var i = 0; i < 20; i++)
                {
                    model.SelectedTheme = model.Themes[i % model.Themes.Count];
                    model.ToggleCompactCommand.Execute(null);
                    await Dispatcher.Yield(DispatcherPriority.Background);
                }
                if (!await model.TryCloseAsync()) throw new Exception("Could not flush preferences.");
                using (var savedStore = new LibraryStore(data))
                {
                    var saved = await savedStore.LoadAsync();
                    if (saved.Preferences.ArtistSpacing != 0 || !saved.Preferences.ReducedMotion || !saved.Preferences.AllowRepeats)
                        throw new Exception("Changed preferences were not persisted.");
                }
                if (errors.Messages.Count != 0) throw new Exception("WPF binding errors: " + string.Join("\n", errors.Messages));
                var process = Process.GetCurrentProcess();
                var initialCpu = process.TotalProcessorTime;
                await Task.Delay(3000);
                process.Refresh();
                var metrics = new { Context = "Windows CI/offscreen smoke, empty library, after captures and 20 theme/view changes; not a production benchmark", PrivateBytes = process.PrivateMemorySize64, WorkingSetBytes = process.WorkingSet64, IdleWindowSeconds = 3, CpuSecondsDuringIdle = (process.TotalProcessorTime - initialCpu).TotalSeconds };
                await File.WriteAllTextAsync(Path.Combine(destination, "metrics.json"), JsonSerializer.Serialize(metrics, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine("PASS: desktop resources, all tabs/themes, compact view, reduced motion, disabled unconfigured auth, preference persistence, no binding errors.");
            }
            catch (Exception error) { exitCode = 1; Console.Error.WriteLine(error); }
            finally { model.Dispose(); app.Shutdown(exitCode); }
        };
        app.Run(window);
        return exitCode;
    }

    private static async Task SettleAsync() { await Dispatcher.Yield(DispatcherPriority.ApplicationIdle); await Task.Delay(150); }
    private static void Capture(Window window, string path)
    {
        var visual = (FrameworkElement)window.Content;
        window.UpdateLayout();
        if (visual.ActualWidth < 1 || visual.ActualHeight < 1) throw new Exception("Empty rendered window.");
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(visual.ActualWidth), (int)Math.Ceiling(visual.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); png.Save(stream);
    }
    private static IEnumerable<DependencyObject> Descendants(DependencyObject node)
    {
        var count = VisualTreeHelper.GetChildrenCount(node);
        for (var i = 0; i < count; i++) { var child = VisualTreeHelper.GetChild(node, i); yield return child; foreach (var item in Descendants(child)) yield return item; }
    }
    private sealed class BindingErrors : TraceListener
    {
        public List<string> Messages { get; } = [];
        public override void Write(string? message) { if (!string.IsNullOrWhiteSpace(message)) Messages.Add(message); }
        public override void WriteLine(string? message) => Write(message);
    }
}
