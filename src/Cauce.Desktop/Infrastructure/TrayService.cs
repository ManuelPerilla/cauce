using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Cauce.Desktop.ViewModels;
using Forms = System.Windows.Forms;

namespace Cauce.Desktop.Infrastructure;

public sealed class TrayService : IDisposable
{
    private const int HotkeyId = 0xCA;
    private readonly Window window;
    private readonly MainViewModel model;
    private readonly Forms.NotifyIcon icon;
    private readonly Forms.ContextMenuStrip menu = new();
    private HwndSource? source;
    private bool hotkeyRegistered;

    public TrayService(Window window, MainViewModel model)
    {
        this.window = window;
        this.model = model;
        menu.Items.Add("Abrir Cauce", null, (_, _) => Restore());
        menu.Items.Add("Reproducir / pausar", null, (_, _) => model.PlayPauseCommand.Execute(null));
        menu.Items.Add("Siguiente", null, (_, _) => model.NextCommand.Execute(null));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Salir", null, (_, _) => window.Close());
        icon = new Forms.NotifyIcon { Text = "Cauce", Icon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath!) ?? System.Drawing.SystemIcons.Application, ContextMenuStrip = menu, Visible = true };
        icon.DoubleClick += (_, _) => Restore();
        window.SourceInitialized += OnSourceInitialized;
        window.StateChanged += OnStateChanged;
        model.PropertyChanged += OnModelChanged;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
        source?.AddHook(HandleMessage);
        hotkeyRegistered = RegisterHotKey(source!.Handle, HotkeyId, 0x0001 | 0x0004 | 0x4000, 0x43); // Alt+Shift+C; no auto-repeat.
    }
    private IntPtr HandleMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == 0x0312 && wParam.ToInt32() == HotkeyId) { Restore(); handled = true; }
        return IntPtr.Zero;
    }
    private void OnStateChanged(object? sender, EventArgs e) { if (window.WindowState == WindowState.Minimized) window.Hide(); }
    private void OnModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.CurrentTitle) or "")
            icon.Text = ("Cauce · " + model.CurrentTitle)[..Math.Min(63, 8 + model.CurrentTitle.Length)];
    }
    private void Restore() { window.Show(); window.WindowState = WindowState.Normal; window.Activate(); }
    public void Dispose()
    {
        if (hotkeyRegistered && source != null) UnregisterHotKey(source.Handle, HotkeyId);
        source?.RemoveHook(HandleMessage);
        window.SourceInitialized -= OnSourceInitialized;
        window.StateChanged -= OnStateChanged;
        model.PropertyChanged -= OnModelChanged;
        icon.Visible = false; icon.Icon?.Dispose(); icon.Dispose(); menu.Dispose();
    }
    [DllImport("user32.dll", SetLastError = true)] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint virtualKey);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
