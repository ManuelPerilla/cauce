using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Win32;
using NgMusic.Shared;

namespace NgMusic.Configurator;

internal sealed class ConfiguratorForm : Form
{
    private const string SetupGuideUrl =
        "https://github.com/ManuelPerilla/cauce/blob/main/docs/configuration.md";

    private readonly TextBox _clientId = new() { Width = 560 };
    private readonly TextBox _clientSecret = new() { Width = 560, UseSystemPasswordChar = true };
    private readonly CheckBox _startMenu = new() { Text = "Create a Start menu shortcut", Checked = true, AutoSize = true };
    private readonly CheckBox _desktop = new() { Text = "Create a desktop shortcut", AutoSize = true };
    private readonly CheckBox _launch = new() { Text = "Launch NgMusic when finished", Checked = true, AutoSize = true };

    public ConfiguratorForm()
    {
        Text = "Finish setting up NgMusic";
        Width = 700;
        Height = 515;
        MinimumSize = MaximumSize = new Size(700, 515);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var title = new Label
        {
            Text = "NgMusic is installed",
            Font = new Font("Segoe UI", 17, FontStyle.Bold),
            AutoSize = true,
            Left = 34,
            Top = 24
        };
        Controls.Add(title);

        var desc = new Label
        {
            Text = "Add your Google OAuth Desktop Client ID now so the next steps are only login → search → play.",
            Font = new Font("Segoe UI", 10),
            AutoSize = false,
            Width = 610,
            Height = 48,
            Left = 34,
            Top = 68
        };
        Controls.Add(desc);

        var label = new Label { Text = "Google OAuth Client ID", AutoSize = true, Left = 34, Top = 128 };
        Controls.Add(label);

        _clientId.Left = 34;
        _clientId.Top = 152;
        _clientId.Text = LoadClientId() ?? string.Empty;
        Controls.Add(_clientId);

        var secretLabel = new Label
        {
            Text = "Google OAuth Client Secret (optional)",
            AutoSize = true,
            Left = 34,
            Top = 194
        };
        Controls.Add(secretLabel);

        _clientSecret.Left = 34;
        _clientSecret.Top = 218;
        Controls.Add(_clientSecret);

        var guide = new Button { Text = "Open setup guide", Left = 34, Top = 258, Width = 150, Height = 30 };
        guide.Click += (_, _) => OpenUrl(SetupGuideUrl);
        Controls.Add(guide);

        _startMenu.Left = 34;
        _startMenu.Top = 302;
        Controls.Add(_startMenu);

        _desktop.Left = 34;
        _desktop.Top = 332;
        Controls.Add(_desktop);

        _launch.Left = 34;
        _launch.Top = 362;
        Controls.Add(_launch);

        var skip = new Button { Text = "Skip for now", Left = 380, Top = 415, Width = 110, Height = 32 };
        skip.Click += (_, _) => Close();
        Controls.Add(skip);

        var save = new Button { Text = "Save && Finish", Left = 500, Top = 415, Width = 130, Height = 32 };
        save.Click += (_, _) => SaveAndFinish();
        Controls.Add(save);
        AcceptButton = save;
    }

    private void SaveAndFinish()
    {
        var clientId = _clientId.Text.Trim();
        if (string.IsNullOrWhiteSpace(clientId))
        {
            MessageBox.Show(
                this,
                "Paste your Google OAuth Desktop Client ID, or choose Skip for now.",
                "Google OAuth Client ID",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!clientId.EndsWith(".apps.googleusercontent.com", StringComparison.OrdinalIgnoreCase))
        {
            var result = MessageBox.Show(
                this,
                "This does not look like a standard Google OAuth Client ID. Save it anyway?",
                "Check Client ID",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;
        }

        SaveClientId(clientId);

        var clientSecret = _clientSecret.Text.Trim();
        if (!string.IsNullOrWhiteSpace(clientSecret))
            new WindowsCredentialSecretStore("NgMusic.GoogleOAuth.ClientSecret").Write(clientSecret);

        var exe = ResolveInstalledExecutable();
        if (!string.IsNullOrWhiteSpace(exe) && File.Exists(exe))
        {
            if (_startMenu.Checked)
                TryCreateShortcut(GetStartMenuShortcutPath(), exe);

            if (_desktop.Checked)
                TryCreateShortcut(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "NgMusic.lnk"),
                    exe);

            if (_launch.Checked)
            {
                try { Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true }); } catch { }
            }
        }

        MessageBox.Show(
            this,
            "NgMusic is ready. Open a terminal and use login → search → play.",
            "NgMusic ready",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);

        Close();
    }

    private static void SaveClientId(string clientId)
    {
        var path = ConfigPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        JsonObject root;
        try
        {
            root = File.Exists(path)
                ? JsonNode.Parse(File.ReadAllText(path)) as JsonObject ?? new JsonObject()
                : new JsonObject();
        }
        catch
        {
            root = new JsonObject();
        }

        root["googleClientId"] = clientId;

        var temp = path + ".tmp";
        File.WriteAllText(temp, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temp, path, overwrite: true);
    }

    private static string? LoadClientId()
    {
        var env = Environment.GetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_ID");
        if (!string.IsNullOrWhiteSpace(env))
            return env;

        var path = ConfigPath();
        if (!File.Exists(path))
            return null;

        try
        {
            var root = JsonNode.Parse(File.ReadAllText(path)) as JsonObject;
            return root?["googleClientId"]?.GetValue<string>();
        }
        catch
        {
            return null;
        }
    }

    private static string ConfigPath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NgMusic",
            "config.json");

    private static string? ResolveInstalledExecutable()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"Software\NgMusic");
            var installPath = key?.GetValue("InstallPath") as string;
            if (!string.IsNullOrWhiteSpace(installPath))
                return Path.Combine(installPath, "ngmusic.exe");
        }
        catch { }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "NgMusic",
            "ngmusic.exe");
    }

    private static string GetStartMenuShortcutPath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            "NgMusic");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, "NgMusic.lnk");
    }

    private static void TryCreateShortcut(string shortcutPath, string targetPath)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(shortcutPath)!);
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is null)
                return;

            dynamic shell = Activator.CreateInstance(shellType)!;
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.Description = "NgMusic";
            shortcut.Save();
        }
        catch { }
    }

    private static void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch
        {
            MessageBox.Show(url, "Open this URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

