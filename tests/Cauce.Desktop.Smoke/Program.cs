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
        var window = new MainWindow { DataContext = model, ShowActivated = false, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.Manual, Left = -16000, Top = -16000 };
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
                AssertGenreSelection(window, model);
                Capture(window, Path.Combine(destination, "01-introduction.png"));
                model.FinishTutorialCommand.Execute(null);
                await SettleAsync();
                model.SelectedTheme = "Claro";
                await SettleAsync();
                Capture(window, Path.Combine(destination, "02-listen-light.png"));
                var tabs = Descendants(window).OfType<TabControl>().Single();
                tabs.SelectedIndex = 1;
                await SettleAsync();
                Capture(window, Path.Combine(destination, "03-library.png"));
                // Synthetic references exercise availability and metadata bindings; no user music is read.
                var samplePath = Path.Combine(data, "sample.wav");
                WriteSilentWave(samplePath);
                var sample = new Track { Id = "sample-local", Title = "Tarde tranquila (prueba)", Artist = "Artista de prueba", Genre = "Jazz", Location = samplePath, Source = TrackSource.LocalFile, IsAvailable = true };
                model.Tracks.Add(sample);
                model.Tracks.Add(new Track { Id = "sample-missing", Title = "Archivo fuera de su carpeta", Artist = "Otro artista", Genre = "Jazz", Location = Path.Combine(data, "missing.mp3"), Source = TrackSource.LocalFile, IsAvailable = false });
                model.Tracks.Add(new Track { Id = "sample-service", Title = "Referencia de servicio", Artist = "music.youtube.com", Genre = "Jazz", Location = "https://music.youtube.com/", Source = TrackSource.ExternalLink, IsAvailable = false });
                model.SelectedTrack = sample;
                model.ApplyGenreCommand.Execute(null);
                await SettleAsync();
                Capture(window, Path.Combine(destination, "03b-library-synthetic.png"));
                tabs.SelectedIndex = 0;
                model.SelectedGenre = "Jazz";
                await SettleAsync();
                AssertGenreSelection(window, model);
                if (!model.QueueReason.Contains("Jazz", StringComparison.OrdinalIgnoreCase)) throw new Exception("Genre queue did not reflect the selected genre.");
                Capture(window, Path.Combine(destination, "02b-listen-synthetic.png"));
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
                model.SelectedTheme = "Bosque";
                if (model.IsCompact) model.ToggleCompactCommand.Execute(null);
                tabs.SelectedIndex = 0;
                await SettleAsync();
                if (!await model.TryCloseAsync()) throw new Exception("Could not flush preferences.");
                using (var savedStore = new LibraryStore(data))
                {
                    var saved = await savedStore.LoadAsync();
                    if (saved.Preferences.ArtistSpacing != 0 || !saved.Preferences.ReducedMotion || !saved.Preferences.AllowRepeats)
                        throw new Exception("Changed preferences were not persisted.");
                }
                if (errors.Messages.Count != 0) throw new Exception("WPF binding errors: " + string.Join("\n", errors.Messages));
                // Let queued rendering and saves settle before sampling; elapsed time is measured, not assumed.
                await Task.Delay(3000);
                var visibleMetrics = await MeasureIdleAsync();
                window.Hide();
                await Task.Delay(2000);
                var hiddenMetrics = await MeasureIdleAsync();
                var metrics = new { Context = "Windows CI/offscreen UI smoke, three synthetic references, no playback, after screenshots and 20 view changes; not a representative hardware benchmark", Visible = visibleMetrics, Hidden = hiddenMetrics };
                var metricsJson = JsonSerializer.Serialize(metrics, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Path.Combine(destination, "metrics.json"), metricsJson);
                Console.WriteLine(metricsJson);
                Console.WriteLine("PASS: desktop resources, all tabs/themes, compact view, genre selection after load/edit, mixed reference library, reduced motion, disabled unconfigured auth, preference persistence, no binding errors.");
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
        // Content is transparent: include the actual Window brush in the render, as the native window does.
        var background = new DrawingVisual();
        using (var drawing = background.RenderOpen()) drawing.DrawRectangle(window.Background, null, new Rect(0, 0, visual.ActualWidth, visual.ActualHeight));
        bitmap.Render(background);
        bitmap.Render(visual);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path); png.Save(stream);
    }
    private static void AssertGenreSelection(Window window, MainViewModel model)
    {
        var picker = (ComboBox)window.FindName("GenrePicker");
        if (!Equals(picker.SelectedItem, model.SelectedGenre) || !Equals(picker.SelectionBoxItem, model.SelectedGenre))
            throw new Exception($"Genre picker lost its selection: model={model.SelectedGenre}, selected={picker.SelectedItem}, displayed={picker.SelectionBoxItem}.");
    }
    private static void WriteSilentWave(string path)
    {
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write("RIFF"u8); writer.Write(36 + 16000); writer.Write("WAVEfmt "u8);
        writer.Write(16); writer.Write((short)1); writer.Write((short)1); writer.Write(8000);
        writer.Write(16000); writer.Write((short)2); writer.Write((short)16);
        writer.Write("data"u8); writer.Write(16000); writer.Write(new byte[16000]);
    }
    private static async Task<object> MeasureIdleAsync()
    {
        using var process = Process.GetCurrentProcess();
        process.Refresh();
        var initialCpu = process.TotalProcessorTime;
        var elapsed = Stopwatch.StartNew();
        await Task.Delay(5000);
        elapsed.Stop(); process.Refresh();
        return new { PrivateBytes = process.PrivateMemorySize64, WorkingSetBytes = process.WorkingSet64, ElapsedSeconds = elapsed.Elapsed.TotalSeconds, CpuSeconds = (process.TotalProcessorTime - initialCpu).TotalSeconds };
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

