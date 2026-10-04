using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Cauce.Core;
using Cauce.Desktop.Infrastructure;
using Cauce.Desktop.Themes;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Cauce.Desktop.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly LibraryStore store;
    private readonly TrackImporter importer = new();
    private readonly QueuePlanner planner = new();
    private readonly NativeAudioPlayer player = new();
    private readonly AuthenticationService authentication = new();
    private readonly CancellationTokenSource lifetime = new();
    private readonly List<string> history = [];
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly DispatcherTimer saveTimer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private AppPreferences preferences = new();
    private Track? selectedTrack;
    private Track? currentTrack;
    private bool initialized;
    private bool disposed;
    private bool writable = true;
    private bool isCompact;
    private bool onboardingVisible;
    private int tutorialIndex;
    private string status = "Añade algunos archivos para empezar. Tu música permanece en su carpeta.";
    private string accountStatus = "Escuchas como invitado. No hace falta una cuenta para tus archivos.";
    private string genreEditText = "";
    private string artistEditText = "";
    private string linkTitle = "";
    private string linkUrl = "";
    private double volume = .6;
    private static readonly (string Title, string Body)[] Tutorial =
    [
        ("¿De dónde viene tu música?", "En Biblioteca puedes añadir MP3, WAV y M4A. Cauce guarda sus referencias, sin copiar los audios. Los enlaces a servicios se guardan para abrirlos en su plataforma."),
        ("¿Qué quieres escuchar hoy?", "Elige un género en Escuchar. Si un archivo no tiene esa información, selecciónalo en Biblioteca y corrígela. El cambio solo afecta a Cauce, nunca al archivo original."),
        ("Una sesión que respeta tu elección", "Siguiente mantiene el género y evita repetir canciones. Si faltan candidatos, te lo explica: puedes permitir repeticiones o cambiar el género. Saltar una canción no modifica tus gustos."),
        ("A tu manera", "En Apariencia puedes elegir un tema, reducir movimiento y usar superficies opacas. Los consejos son opcionales. Puedes repetir esta guía y exportar o borrar tus datos desde Cuenta.")
    ];

    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<Track> Tracks { get; } = [];
    public ObservableCollection<string> Genres { get; } = ["Todos"];
    public IReadOnlyList<string> Themes { get; } = ["Sistema", "Claro", "Oscuro", "Bosque", "Alto contraste"];
    public Track? SelectedTrack { get => selectedTrack; set { selectedTrack = value; GenreEditText = value?.Genre ?? ""; ArtistEditText = value?.Artist ?? ""; Changed(); } }
    public string GenreEditText { get => genreEditText; set { genreEditText = value; Changed(); } }
    public string ArtistEditText { get => artistEditText; set { artistEditText = value; Changed(); } }
    public string LinkTitle { get => linkTitle; set { linkTitle = value; Changed(); } }
    public string LinkUrl { get => linkUrl; set { linkUrl = value; Changed(); } }
    public string SelectedGenre { get => preferences.Genre; set { if (string.IsNullOrWhiteSpace(value)) return; preferences = preferences with { Genre = value }; history.Clear(); Changed(); Changed(nameof(QueueReason)); ScheduleSave(); } }
    public string SelectedTheme { get => preferences.Theme; set { if (!Themes.Contains(value)) return; preferences = preferences with { Theme = value }; ApplyAppearance(); Changed(); ScheduleSave(); } }
    public bool ReducedMotion { get => preferences.ReducedMotion; set { preferences = preferences with { ReducedMotion = value }; Changed(); Changed(nameof(MotionEnabled)); ScheduleSave(); } }
    public bool ReducedTransparency { get => preferences.ReducedTransparency; set { preferences = preferences with { ReducedTransparency = value }; ApplyAppearance(); Changed(); ScheduleSave(); } }
    public bool TipsEnabled { get => preferences.TipsEnabled; set { preferences = preferences with { TipsEnabled = value }; Changed(); ScheduleSave(); } }
    public bool AllowRepeats { get => preferences.AllowRepeats; set { preferences = preferences with { AllowRepeats = value }; Changed(); Changed(nameof(QueueReason)); ScheduleSave(); } }
    public int ArtistSpacing { get => preferences.ArtistSpacing; set { preferences = preferences with { ArtistSpacing = Math.Clamp(value, 0, 5) }; Changed(); Changed(nameof(QueueReason)); ScheduleSave(); } }
    public bool MotionEnabled => !ReducedMotion && SystemParameters.ClientAreaAnimation && !SystemParameters.HighContrast;
    public bool IsCompact => isCompact;
    public double Volume { get => volume; set { volume = Math.Clamp(value, 0, 1); player.Volume = volume; Changed(); } }
    public double PlaybackProgress => player.Duration.TotalSeconds > 0 ? Math.Clamp(player.Position.TotalSeconds / player.Duration.TotalSeconds, 0, 1) : 0;
    public string PlaybackTime => $"{player.Position:mm\\:ss} / {player.Duration:mm\\:ss}";
    public string CurrentTitle => currentTrack?.Title ?? "Tu próxima sesión empieza aquí";
    public string CurrentArtist => currentTrack is null ? "Añade música y elige una línea para escuchar" : string.IsNullOrWhiteSpace(currentTrack.Artist) ? "Artista sin identificar" : currentTrack.Artist;
    public string PlaybackLabel => player.IsPlaying ? "Pausar" : "Reproducir";
    public string QueueReason => planner.Next(Tracks.ToList(), Rules(), history).Reason;
    public string Status { get => status; private set { status = value; Changed(); } }
    public string StorageSummary => $"{Tracks.Count} referencias guardadas · audios en su ubicación original · sin historial persistente ni telemetría";
    public string AccountStatus { get => accountStatus; private set { accountStatus = value; Changed(); } }
    public bool CanSignIn => authentication.IsConfigured;
    public bool OnboardingVisible { get => onboardingVisible; private set { onboardingVisible = value; Changed(); } }
    public string TutorialTitle => Tutorial[tutorialIndex].Title;
    public string TutorialBody => Tutorial[tutorialIndex].Body;
    public string TutorialProgress => $"{tutorialIndex + 1} de {Tutorial.Length}";

    public ICommand ImportCommand { get; }
    public ICommand PlayPauseCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PlaySelectedCommand { get; }
    public ICommand RemoveTrackCommand { get; }
    public ICommand ApplyGenreCommand { get; }
    public ICommand AddLinkCommand { get; }
    public ICommand OpenExternalCommand { get; }
    public ICommand ReplayTutorialCommand { get; }
    public ICommand FinishTutorialCommand { get; }
    public ICommand NextTutorialCommand { get; }
    public ICommand PreviousTutorialCommand { get; }
    public ICommand ExportDataCommand { get; }
    public ICommand ClearDataCommand { get; }
    public ICommand OpenBugReportCommand { get; }
    public ICommand OpenSupportCommand { get; }
    public ICommand SignInCommand { get; }
    public ICommand SignOutCommand { get; }
    public ICommand ToggleCompactCommand { get; }

    public MainViewModel(string? dataDirectory = null)
    {
        store = new LibraryStore(dataDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cauce"));
        ImportCommand = Async(_ => ImportAsync());
        PlayPauseCommand = Action(_ => PlayPause());
        NextCommand = Action(_ => PlayNext());
        PlaySelectedCommand = Action(_ => { if (SelectedTrack is not null) StartTrack(SelectedTrack); });
        RemoveTrackCommand = Async(_ => RemoveAsync());
        ApplyGenreCommand = Async(_ => ApplyGenreAsync());
        AddLinkCommand = Async(_ => AddLinkAsync());
        OpenExternalCommand = Action(_ => OpenSelectedLink());
        ReplayTutorialCommand = Action(_ => { tutorialIndex = 0; OnboardingVisible = true; NotifyTutorial(); });
        FinishTutorialCommand = Async(_ => FinishTutorialAsync());
        NextTutorialCommand = Action(_ => { tutorialIndex = Math.Min(tutorialIndex + 1, Tutorial.Length - 1); NotifyTutorial(); });
        PreviousTutorialCommand = Action(_ => { tutorialIndex = Math.Max(0, tutorialIndex - 1); NotifyTutorial(); });
        ExportDataCommand = Async(_ => ExportAsync());
        ClearDataCommand = Async(_ => ClearAsync());
        OpenBugReportCommand = Action(_ => OpenBugReport());
        OpenSupportCommand = Action(_ => OpenHttps("https://github.com/ManuelPerilla/cauce/issues"));
        SignInCommand = Async(async provider =>
        {
            if (!authentication.IsConfigured) { AccountStatus = authentication.ConfigurationStatus; return; }
            AccountStatus = "Completa el acceso en tu navegador. Puedes cerrar el navegador para cancelar.";
            try { var name = await authentication.SignInAsync(provider?.ToString() ?? "", lifetime.Token); AccountStatus = $"Sesión iniciada: {name}. La biblioteca sigue siendo local; sincronización aún no disponible."; }
            catch (OperationCanceledException) { AccountStatus = "Acceso cancelado. Puedes seguir escuchando como invitado."; }
            catch (Exception) { AccountStatus = "No se pudo completar el acceso. Revisa la conexión y la configuración del proveedor; puedes volver a intentarlo."; }
        });
        SignOutCommand = Action(_ => { authentication.SignOut(); AccountStatus = "Sesión cerrada. Tu biblioteca local permanece en este equipo."; });
        ToggleCompactCommand = Action(_ =>
        {
            isCompact = !isCompact;
            var window = Application.Current.MainWindow;
            if (window is not null)
            {
                window.MinWidth = isCompact ? 440 : 850;
                window.MinHeight = isCompact ? 260 : 650;
                window.Width = isCompact ? 510 : 1060;
                window.Height = isCompact ? 310 : 760;
            }
            Changed(nameof(IsCompact));
        });
        player.Volume = volume;
        player.Opened += () =>
        {
            if (currentTrack != null) history.Add(currentTrack.Id);
            if (AllowRepeats && history.Count > 256) history.RemoveRange(0, history.Count - 256);
            timer.Start(); NotifyPlayer(); Status = "Tu sesión está en marcha.";
        };
        player.Ended += PlayNext;
        player.Failed += () =>
        {
            timer.Stop();
            if (currentTrack != null)
            {
                var index = Tracks.ToList().FindIndex(t => t.Id == currentTrack.Id);
                if (index >= 0) Tracks[index] = Tracks[index] with { IsAvailable = false };
            }
            Status = "No se pudo abrir este audio. Se excluye de esta sesión; comprueba su formato o vuelve a añadirlo para reintentar.";
            NotifyPlayer();
        };
        timer.Tick += (_, _) => { Changed(nameof(PlaybackProgress)); Changed(nameof(PlaybackTime)); };
        saveTimer.Tick += async (_, _) => { saveTimer.Stop(); try { await SaveAsync(); } catch (Exception) { Status = "No se pudieron guardar los cambios. Exporta tu biblioteca desde Soporte antes de cerrar."; } };
        SystemParameters.StaticPropertyChanged += OnSystemPreferencesChanged;
        ApplyAppearance();
    }

    public async Task InitializeAsync()
    {
        try
        {
            var saved = await store.LoadAsync(lifetime.Token);
            preferences = saved.Preferences;
            foreach (var track in saved.Tracks) Tracks.Add(track.Source == TrackSource.LocalFile ? track with { IsAvailable = File.Exists(track.Location) } : track);
            RebuildGenres();
            if (!Themes.Contains(preferences.Theme)) preferences = preferences with { Theme = "Sistema" };
            if (!Genres.Contains(preferences.Genre)) preferences = preferences with { Genre = "Todos" };
            ApplyAppearance();
            if (!string.IsNullOrEmpty(store.LastLoadWarning)) Status = store.LastLoadWarning;
            else if (Tracks.Count > 0) Status = "Tu biblioteca está lista. Elige una canción o empieza una sesión.";
        }
        catch (Exception)
        {
            writable = false;
            Status = "No se pudo leer la biblioteca. Se conserva intacta y se desactiva el guardado para protegerla.";
        }
        initialized = true;
        OnboardingVisible = !preferences.OnboardingCompleted;
        if (!authentication.IsConfigured) AccountStatus = authentication.ConfigurationStatus;
        Changed("");
    }

    private SessionRules Rules() => new() { Genre = SelectedGenre, AllowRepeats = AllowRepeats, ArtistSpacing = ArtistSpacing };
    private async Task ImportAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog { Title = "Añadir música a Cauce", Filter = "Audio compatible|*.mp3;*.wav;*.m4a", Multiselect = true, CheckFileExists = true };
        if (dialog.ShowDialog() != true) return;
        Status = "Leyendo la información de tus archivos…";
        var imported = await importer.ImportFilesAsync(dialog.FileNames, lifetime.Token);
        var known = Tracks.Select(t => t.Id).ToHashSet(StringComparer.Ordinal);
        var added = 0;
        foreach (var track in imported)
        {
            if (!known.Contains(track.Id) && Tracks.Count >= LibraryStore.MaximumTracks) break;
            if (known.Add(track.Id)) { Tracks.Add(track); added++; }
            else { var old = Tracks.First(t => t.Id == track.Id); Tracks[Tracks.IndexOf(old)] = old with { IsAvailable = track.IsAvailable }; }
        }
        RebuildGenres(); await SaveAsync(); NotifyCollection();
        Status = $"{added} archivos añadidos. Puedes corregir el género sin modificar el audio original.";
    }

    private void PlayPause()
    {
        if (player.IsPlaying) { player.Pause(); timer.Stop(); NotifyPlayer(); return; }
        if (player.HasTrack) { player.Resume(); timer.Start(); NotifyPlayer(); return; }
        if (SelectedTrack is { Source: TrackSource.LocalFile } track) StartTrack(track);
        else PlayNext();
    }

    private void PlayNext()
    {
        var choice = planner.Next(Tracks.ToList(), Rules(), history);
        if (choice.Track == null) { player.Pause(); timer.Stop(); Status = choice.Reason; NotifyPlayer(); return; }
        StartTrack(choice.Track);
    }

    private void StartTrack(Track track)
    {
        if (track.Source != TrackSource.LocalFile) { Status = "Este enlace se abre en su plataforma y no forma parte de la cola de audio local."; return; }
        if (!File.Exists(track.Location))
        {
            var index = Tracks.IndexOf(track);
            if (index >= 0) Tracks[index] = track with { IsAvailable = false };
            Status = "El archivo ya no está en esa ubicación. Añádelo de nuevo desde su carpeta actual.";
            Changed(nameof(QueueReason)); return;
        }
        currentTrack = track;
        player.Open(track.Location); Status = "Abriendo audio…"; NotifyPlayer();
    }

    private async Task RemoveAsync()
    {
        if (SelectedTrack == null) { Status = "Selecciona una referencia en Biblioteca."; return; }
        if (currentTrack?.Id == SelectedTrack.Id) { player.Stop(); timer.Stop(); currentTrack = null; NotifyPlayer(); }
        Tracks.Remove(SelectedTrack); SelectedTrack = null; RebuildGenres(); await SaveAsync(); NotifyCollection();
        Status = "Referencia retirada. El archivo original no se ha borrado.";
    }

    private async Task ApplyGenreAsync()
    {
        if (SelectedTrack == null) { Status = "Selecciona primero una canción."; return; }
        var genre = GenreEditText.Trim();
        if (genre.Length is 0 or > 80) { Status = "Escribe un género de entre 1 y 80 caracteres."; return; }
        if (ArtistEditText.Length > 200) { Status = "El nombre del artista debe tener como máximo 200 caracteres."; return; }
        var updated = SelectedTrack with { Genre = genre, Artist = ArtistEditText.Trim() };
        Tracks[Tracks.IndexOf(SelectedTrack)] = updated; SelectedTrack = updated;
        if (currentTrack?.Id == updated.Id) { currentTrack = updated; NotifyPlayer(); }
        RebuildGenres(); await SaveAsync(); NotifyCollection(); Status = "Datos actualizados solo en tu biblioteca de Cauce.";
    }

    private async Task AddLinkAsync()
    {
        if (!Uri.TryCreate(LinkUrl.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo) || LinkUrl.Length > 2048)
        { Status = "Usa un enlace HTTPS público, sin credenciales ni enlaces de acceso privados."; return; }
        var title = LinkTitle.Trim();
        if (title.Length is 0 or > 200) { Status = "Escribe un título de entre 1 y 200 caracteres."; return; }
        if (Tracks.Any(t => t.Source == TrackSource.ExternalLink && t.Location == uri.AbsoluteUri)) { Status = "Ese enlace ya está guardado."; return; }
        if (Tracks.Count >= LibraryStore.MaximumTracks) { Status = "Esta biblioteca alcanzó su límite de 10.000 referencias. Retira alguna antes de añadir más."; return; }
        Tracks.Add(new Track { Id = Guid.NewGuid().ToString("N"), Title = title, Artist = uri.Host, Genre = "Sin clasificar", Source = TrackSource.ExternalLink, Location = uri.AbsoluteUri, IsAvailable = false });
        LinkTitle = ""; LinkUrl = ""; RebuildGenres(); await SaveAsync(); NotifyCollection();
        Status = "Enlace guardado. Su disponibilidad no está comprobada; se abre en su plataforma.";
    }

    private void OpenSelectedLink()
    {
        if (SelectedTrack is not { Source: TrackSource.ExternalLink } track) { Status = "Selecciona un enlace de servicio en Biblioteca."; return; }
        OpenHttps(track.Location);
    }

    private async Task FinishTutorialAsync()
    { OnboardingVisible = false; preferences = preferences with { OnboardingCompleted = true }; await SaveAsync(); }
    private void NotifyTutorial() { Changed(nameof(TutorialTitle)); Changed(nameof(TutorialBody)); Changed(nameof(TutorialProgress)); }
    private void RebuildGenres()
    {
        var all = Tracks.Select(t => t.Genre)
            .Where(g => !string.IsNullOrWhiteSpace(g) && !g.Equals("Todos", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.CurrentCultureIgnoreCase).ToList();
        all.Insert(0, "Todos");
        var previous = preferences.Genre;
        var desired = new HashSet<string>(all, StringComparer.OrdinalIgnoreCase);
        // Preserve existing items so the ComboBox does not lose its selected item during a reset.
        for (var i = Genres.Count - 1; i >= 0; i--)
            if (!desired.Contains(Genres[i])) Genres.RemoveAt(i);
        for (var i = 0; i < all.Count; i++)
        {
            var existing = -1;
            for (var j = i; j < Genres.Count; j++)
                if (Genres[j].Equals(all[i], StringComparison.OrdinalIgnoreCase)) { existing = j; break; }
            if (existing < 0) Genres.Insert(i, all[i]);
            else if (existing != i) Genres.Move(existing, i);
        }
        var selected = Genres.FirstOrDefault(g => g.Equals(previous, StringComparison.OrdinalIgnoreCase)) ?? "Todos";
        preferences = preferences with { Genre = selected };
        Changed(nameof(SelectedGenre));
    }
    private LibraryState Snapshot() => new() { Tracks = Tracks.ToList(), Preferences = preferences };
    private Task SaveAsync() => writable ? store.SaveAsync(Snapshot(), lifetime.Token) : Task.CompletedTask;
    private void ScheduleSave() { if (initialized && !disposed) { saveTimer.Stop(); saveTimer.Start(); } }
    private void ApplyAppearance() => ThemeManager.Apply(SelectedTheme, ReducedTransparency);
    private void OnSystemPreferencesChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(SystemParameters.ClientAreaAnimation) and not nameof(SystemParameters.HighContrast)) return;
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.HasShutdownStarted) return;
        dispatcher.InvokeAsync(() => { if (!disposed) Changed(nameof(MotionEnabled)); });
    }

    private async Task ExportAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog { FileName = "cauce-biblioteca.json", Filter = "Biblioteca JSON|*.json" };
        if (dialog.ShowDialog() != true) return;
        await File.WriteAllTextAsync(dialog.FileName, JsonSerializer.Serialize(Snapshot(), new JsonSerializerOptions { WriteIndented = true }), lifetime.Token);
        Status = "Biblioteca exportada. Incluye rutas y enlaces; compártela solo si quieres compartir esa información.";
    }

    private async Task ClearAsync()
    {
        if (MessageBox.Show("Se retirarán las referencias, preferencias y la sesión de Cauce de este equipo. Tus archivos de música no se borrarán. ¿Continuar?", "Borrar datos locales", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        player.Stop(); timer.Stop(); currentTrack = null; Tracks.Clear(); history.Clear();
        authentication.SignOut(); AccountStatus = "Escuchas como invitado.";
        preferences = new AppPreferences { OnboardingCompleted = true }; RebuildGenres(); ApplyAppearance();
        await store.ClearAsync(lifetime.Token);
        await SaveAsync(); Changed(""); Status = "Datos locales restablecidos. Tus archivos de música siguen intactos.";
    }

    private void OpenBugReport()
    {
        var version = typeof(MainViewModel).Assembly.GetName().Version?.ToString() ?? "desconocida";
        var body = $"### Qué ocurrió\n\n### Pasos para reproducir\n1. \n\n### Qué esperabas\n\n### Entorno\nCauce: {version}\nWindows: {Environment.OSVersion.Version}\n\nRevisa lo que compartes. No incluyas contraseñas, tokens, rutas personales ni archivos de música.\n";
        OpenHttps("https://github.com/ManuelPerilla/cauce/issues/new?title=" + Uri.EscapeDataString("[Cauce] ") + "&body=" + Uri.EscapeDataString(body));
        Status = "Se abrió un borrador en GitHub. Revísalo y envíalo cuando esté listo; Cauce no adjunta archivos ni envía datos automáticamente.";
    }
    private static void OpenHttps(string location)
    {
        if (!Uri.TryCreate(location, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo)) throw new InvalidOperationException("Unsupported link.");
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
    }
    private AsyncCommand Async(Func<object?, Task> action) => new(action, _ => Status = "No se pudo completar la acción. Tus audios originales permanecen intactos. Puedes volver a intentarlo.");
    private RelayCommand Action(Action<object?> action) => new(parameter => { try { action(parameter); } catch (Exception) { Status = "No se pudo completar la acción. Revisa la selección e inténtalo de nuevo."; } });
    private void NotifyCollection() { Changed(nameof(StorageSummary)); Changed(nameof(QueueReason)); }
    private void NotifyPlayer() { Changed(nameof(CurrentTitle)); Changed(nameof(CurrentArtist)); Changed(nameof(PlaybackLabel)); Changed(nameof(PlaybackProgress)); Changed(nameof(PlaybackTime)); Changed(nameof(QueueReason)); }
    private void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public async Task<bool> TryCloseAsync()
    {
        saveTimer.Stop();
        if (!initialized || disposed || !writable) return true;
        try { await SaveAsync(); return true; }
        catch (Exception)
        {
            return MessageBox.Show("No se pudieron guardar los últimos cambios. Puedes volver a la app y exportar tu biblioteca. ¿Cerrar de todos modos?", "Cambios pendientes", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true; timer.Stop(); saveTimer.Stop();
        SystemParameters.StaticPropertyChanged -= OnSystemPreferencesChanged;
        lifetime.Cancel(); player.Dispose(); authentication.Dispose(); lifetime.Dispose(); store.Dispose();
    }
}

