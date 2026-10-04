using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Win32;
using NgMusic.Shared;

namespace NgMusic.Setup;

internal sealed class WizardForm : Form
{
    private const string ResourceName = "NgMusic.Setup.Payload.msi";
    private const string SetupGuideUrl =
        "https://github.com/ManuelPerilla/cauce/blob/main/docs/configuration.md";

    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new Padding(36, 22, 36, 16) };
    private readonly Button _back = new() { Text = "< Back", Width = 95, Height = 32 };
    private readonly Button _next = new() { Text = "Next >", Width = 95, Height = 32 };
    private readonly Button _cancel = new() { Text = "Cancel", Width = 95, Height = 32 };

    private readonly TextBox _clientId = new() { Width = 590 };
    private readonly TextBox _clientSecret = new() { Width = 590, UseSystemPasswordChar = true };
    private readonly CheckBox _startMenu = new() { Text = "Create a Start menu shortcut", Checked = true, AutoSize = true };
    private readonly CheckBox _desktop = new() { Text = "Create a desktop shortcut", Checked = false, AutoSize = true };
    private readonly CheckBox _launch = new() { Text = "Launch NgMusic when setup finishes", Checked = true, AutoSize = true };
    private readonly Label _installStatus = new() { AutoSize = true, Text = "Ready to install." };
    private readonly ProgressBar _progress = new() { Width = 590, Height = 22, Style = ProgressBarStyle.Marquee, Visible = false };

    private readonly List<Control> _pages;
    private int _pageIndex;
    private bool _installing;
    private bool _completed;
    private string? _installedExe;

    public WizardForm()
    {
        Text = "NgMusic Setup";
        Width = 760;
        Height = 540;
        MinimumSize = MaximumSize = new Size(760, 540);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var header = new Panel { Dock = DockStyle.Top, Height = 82, BackColor = Color.FromArgb(18, 18, 18) };
        var title = new Label
        {
            Text = "NgMusic Setup",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            Left = 28,
            Top = 14
        };
        var subtitle = new Label
        {
            Text = "Terminal-first music for Windows",
            ForeColor = Color.Gainsboro,
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Left = 30,
            Top = 50
        };
        header.Controls.Add(title);
        header.Controls.Add(subtitle);

        var nav = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.FromArgb(245, 245, 245) };
        _cancel.Left = 28;
        _cancel.Top = 14;
        _back.Left = 520;
        _back.Top = 14;
        _next.Left = 620;
        _next.Top = 14;
        nav.Controls.AddRange([_cancel, _back, _next]);

        Controls.Add(_content);
        Controls.Add(nav);
        Controls.Add(header);

        _pages =
        [
            BuildWelcomePage(),
            BuildOAuthPage(),
            BuildOptionsPage(),
            BuildInstallPage()
        ];

        _back.Click += (_, _) => ShowPage(_pageIndex - 1);
        _next.Click += async (_, _) => await NextAsync();
        _cancel.Click += (_, _) => Close();
        FormClosing += OnFormClosing;

        _clientId.Text = LoadInitialClientId() ?? string.Empty;
        ShowPage(0);
    }

    private Control BuildWelcomePage()
    {
        var panel = NewPage();
        var y = 8;

        AddHeading(panel, "Welcome to NgMusic", ref y);
        AddParagraph(panel,
            "This guided installer prepares NgMusic so your first session can be just login, search and play.",
            ref y);
        AddParagraph(panel,
            "The application is self-contained, so no separate .NET runtime is required. Setup will install NgMusic, add it to PATH and collect the non-secret Google OAuth Desktop Client ID used for login.",
            ref y);
        AddParagraph(panel,
            "You will need a Google OAuth Client ID of type Desktop app. If you do not have one yet, the next page can open the step-by-step guide.",
            ref y);

        return panel;
    }

    private Control BuildOAuthPage()
    {
        var panel = NewPage();
        var y = 8;

        AddHeading(panel, "Google OAuth", ref y);
        AddParagraph(panel,
            "Paste the Client ID from a Google OAuth application of type Desktop app. It normally ends in .apps.googleusercontent.com.",
            ref y);

        var label = new Label { Text = "Google OAuth Client ID", AutoSize = true, Left = 0, Top = y };
        panel.Controls.Add(label);
        y += 24;

        _clientId.Left = 0;
        _clientId.Top = y;
        panel.Controls.Add(_clientId);
        y += 45;

        var secretLabel = new Label
        {
            Text = "Google OAuth Client Secret (optional)",
            AutoSize = true,
            Left = 0,
            Top = y
        };
        panel.Controls.Add(secretLabel);
        y += 24;

        _clientSecret.Left = 0;
        _clientSecret.Top = y;
        panel.Controls.Add(_clientSecret);
        y += 45;

        var guide = new Button { Text = "Open setup guide", Width = 150, Height = 32, Left = 0, Top = y };
        guide.Click += (_, _) => OpenUrl(SetupGuideUrl);
        panel.Controls.Add(guide);
        y += 48;

        AddParagraph(panel,
            "Client ID is stored in local config. If Google provides a Client Secret, NgMusic stores it securely in Windows Credential Manager. OAuth tokens also stay there. No Google password is stored.",
            ref y);

        return panel;
    }

    private Control BuildOptionsPage()
    {
        var panel = NewPage();
        var y = 8;

        AddHeading(panel, "Installation options", ref y);
        AddParagraph(panel,
            "NgMusic will be installed in Program Files and added to the system PATH. Windows will request administrator approval only when installation begins.",
            ref y);

        _startMenu.Left = 0;
        _startMenu.Top = y;
        panel.Controls.Add(_startMenu);
        y += 34;

        _desktop.Left = 0;
        _desktop.Top = y;
        panel.Controls.Add(_desktop);
        y += 34;

        _launch.Left = 0;
        _launch.Top = y;
        panel.Controls.Add(_launch);
        y += 48;

        AddParagraph(panel,
            "After setup, open a terminal and type ngmusic. Your Google Client ID will already be configured, so the normal flow is login → search → play.",
            ref y);

        return panel;
    }

    private Control BuildInstallPage()
    {
        var panel = NewPage();
        var y = 8;

        AddHeading(panel, "Install NgMusic", ref y);

        _installStatus.Left = 0;
        _installStatus.Top = y;
        _installStatus.Font = new Font("Segoe UI", 10);
        panel.Controls.Add(_installStatus);
        y += 38;

        _progress.Left = 0;
        _progress.Top = y;
        panel.Controls.Add(_progress);

        return panel;
    }

    private static Panel NewPage() => new() { Dock = DockStyle.Fill, Visible = false };

    private static void AddHeading(Control parent, string text, ref int y)
    {
        var label = new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 17, FontStyle.Bold),
            AutoSize = true,
            Left = 0,
            Top = y
        };
        parent.Controls.Add(label);
        y += 46;
    }

    private static void AddParagraph(Control parent, string text, ref int y)
    {
        var label = new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10),
            AutoSize = false,
            Width = 630,
            Height = 70,
            Left = 0,
            Top = y
        };
        parent.Controls.Add(label);
        y += 82;
    }

    private void ShowPage(int index)
    {
        if (index < 0 || index >= _pages.Count)
            return;

        _content.Controls.Clear();
        _pageIndex = index;
        _content.Controls.Add(_pages[index]);
        _pages[index].Visible = true;

        _back.Enabled = index > 0 && !_installing;
        _cancel.Enabled = !_installing;
        _next.Enabled = !_installing;
        _next.Text = index switch
        {
            2 => "Install",
            3 when _completed => "Finish",
            _ => "Next >"
        };
    }

    private async Task NextAsync()
    {
        if (_pageIndex == 0)
        {
            ShowPage(1);
            return;
        }

        if (_pageIndex == 1)
        {
            if (!ValidateClientId())
                return;

            ShowPage(2);
            return;
        }

        if (_pageIndex == 2)
        {
            ShowPage(3);
            await InstallAsync();
            return;
        }

        if (_pageIndex == 3 && _completed)
        {
            if (_launch.Checked && !string.IsNullOrWhiteSpace(_installedExe) && File.Exists(_installedExe))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(_installedExe) { UseShellExecute = true });
                }
                catch
                {
                    // Installation succeeded; launching is best-effort.
                }
            }

            Close();
        }
    }

    private bool ValidateClientId()
    {
        var value = _clientId.Text.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            MessageBox.Show(
                this,
                "Enter your Google OAuth Desktop Client ID before continuing.",
                "Google OAuth required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        if (!value.EndsWith(".apps.googleusercontent.com", StringComparison.OrdinalIgnoreCase))
        {
            var result = MessageBox.Show(
                this,
                "This value does not look like a standard Google OAuth Client ID. Continue anyway?",
                "Check Client ID",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        return true;
    }

    private async Task InstallAsync()
    {
        _installing = true;
        _completed = false;
        _progress.Visible = true;
        _installStatus.Text = "Preparing installer...";
        UpdateNavigation();

        string? tempDirectory = null;

        try
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), "NgMusicSetup", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);
            var msiPath = Path.Combine(tempDirectory, "NgMusic.msi");

            await ExtractEmbeddedMsiAsync(msiPath);

            _installStatus.Text = "Waiting for Windows installation approval...";
            var exitCode = await InstallMsiAsync(msiPath);

            if (exitCode is not 0 and not 3010)
                throw new InvalidOperationException($"Windows Installer exited with code {exitCode}.");

            _installStatus.Text = "Saving Google OAuth configuration...";
            SaveClientId(_clientId.Text.Trim());

            var clientSecret = _clientSecret.Text.Trim();
            if (!string.IsNullOrWhiteSpace(clientSecret))
                new WindowsCredentialSecretStore("NgMusic.GoogleOAuth.ClientSecret").Write(clientSecret);

            _installedExe = ResolveInstalledExecutable();
            if (!string.IsNullOrWhiteSpace(_installedExe) && File.Exists(_installedExe))
            {
                if (_startMenu.Checked)
                    TryCreateShortcut(GetStartMenuShortcutPath(), _installedExe);

                if (_desktop.Checked)
                    TryCreateShortcut(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "NgMusic.lnk"),
                        _installedExe);
            }

            _installStatus.Text =
                "NgMusic is ready. Open a terminal and run ngmusic, then use login → search → play.";
            _completed = true;
            _progress.Visible = false;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            _installStatus.Text = "Installation was cancelled at the Windows administrator prompt.";
            _progress.Visible = false;
        }
        catch (Exception ex)
        {
            _installStatus.Text = "Setup could not complete.";
            _progress.Visible = false;
            MessageBox.Show(this, ex.Message, "NgMusic Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _installing = false;
            if (!string.IsNullOrWhiteSpace(tempDirectory))
            {
                try { Directory.Delete(tempDirectory, recursive: true); } catch { }
            }

            UpdateNavigation();
        }
    }

    private void UpdateNavigation()
    {
        _back.Enabled = !_installing && _pageIndex > 0 && !_completed;
        _cancel.Enabled = !_installing && !_completed;
        _next.Enabled = !_installing;
        _next.Text = _completed ? "Finish" : _pageIndex == 2 ? "Install" : "Next >";
    }

    private static async Task ExtractEmbeddedMsiAsync(string outputPath)
    {
        await using var input = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException("The embedded NgMusic MSI payload is missing.");

        await using var output = File.Create(outputPath);
        await input.CopyToAsync(output);
    }

    private static async Task<int> InstallMsiAsync(string msiPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "msiexec.exe",
            Arguments = $"/i \"{msiPath}\" NGMUSIC_WRAPPER=1 /qn /norestart",
            UseShellExecute = true,
            Verb = "runas"
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Windows Installer could not be started.");

        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    private static void SaveClientId(string clientId)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NgMusic");
        var path = Path.Combine(directory, "config.json");

        Directory.CreateDirectory(directory);

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

    private static string? LoadInitialClientId()
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("NGMUSIC_GOOGLE_CLIENT_ID");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
            return fromEnvironment;

        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NgMusic",
            "config.json");

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

    private static string? ResolveInstalledExecutable()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"Software\NgMusic");
            var installPath = key?.GetValue("InstallPath") as string;
            if (!string.IsNullOrWhiteSpace(installPath))
                return Path.Combine(installPath, "ngmusic.exe");
        }
        catch
        {
            // Fall back to the conventional location.
        }

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
        catch
        {
            // Shortcut creation is optional and must not fail setup.
        }
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            MessageBox.Show(url, "Open this URL", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!_installing)
            return;

        e.Cancel = true;
        MessageBox.Show(
            this,
            "Please wait for the installation to finish.",
            "NgMusic Setup",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}

