using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace Cauce.Desktop.Themes;

/// <summary>Changes frozen brushes in place so existing controls update without recreating the window.</summary>
public static class ThemeManager
{
    private static string requestedTheme = "Sistema";
    private static bool reducedTransparency;

    static ThemeManager()
    {
        SystemParameters.StaticPropertyChanged += OnSystemPropertyChanged;
        SystemEvents.UserPreferenceChanged += (_, _) => RefreshOnUiThread();
    }

    public static void Apply(string theme, bool reduceTransparency)
    {
        requestedTheme = theme;
        reducedTransparency = reduceTransparency;
        var application = Application.Current;
        if (application is null) return;
        if (!application.Dispatcher.CheckAccess())
        {
            application.Dispatcher.Invoke(() => Apply(theme, reduceTransparency));
            return;
        }

        if (SystemParameters.HighContrast || theme == "Alto contraste")
        {
            SetHighContrast(application.Resources, SystemParameters.HighContrast);
            return;
        }

        var dark = theme == "Oscuro" || theme == "Bosque" || (theme == "Sistema" && !IsSystemLight());
        var forest = theme == "Bosque";
        var resources = application.Resources;
        SetBrush(resources, "WindowBackground", forest ? "#15251F" : dark ? "#161A22" : "#F3F2ED");
        SetBrush(resources, "SolidSurfaceBrush", forest ? "#20352B" : dark ? "#222833" : "#FFFFFF");
        SetBrush(resources, "SurfaceBrush", forest ? (reduceTransparency ? "#243B30" : "#D9243B30") : dark ? (reduceTransparency ? "#252C38" : "#D9252C38") : (reduceTransparency ? "#FFFFFF" : "#D9FFFFFF"));
        SetBrush(resources, "SubtleSurfaceBrush", forest ? "#293E32" : dark ? "#2B3240" : "#EAEDE7");
        SetBrush(resources, "BorderBrush", forest ? "#4B6354" : dark ? "#46505F" : "#CCD3C8");
        SetBrush(resources, "TextPrimaryBrush", dark ? "#F2F4EF" : "#243A30");
        SetBrush(resources, "TextSecondaryBrush", dark ? "#C0CAC3" : "#526159");
        SetBrush(resources, "AccentBrush", dark ? "#B8D7A8" : "#365B46");
        SetBrush(resources, "AccentTextBrush", dark ? "#162B1C" : "#FFFFFF");
        SetBrush(resources, "AccentSoftBrush", forest ? "#3A5140" : dark ? "#374739" : "#E0EBD9");
        SetBrush(resources, "SelectionBrush", dark ? "#3C5945" : "#DCEBD8");
        SetBrush(resources, "SelectionTextBrush", dark ? "#F2F4EF" : "#243A30");
        SetBrush(resources, "OverlayBrush", reduceTransparency ? (dark ? "#161A22" : "#E5E9E1") : "#880C1711");
        SetBrush(resources, "WarmBrush", dark ? "#655D48" : "#EFE3C9");
        if (reduceTransparency)
            resources["GlassHighlightBrush"] = resources["SolidSurfaceBrush"];
        else
        {
            var highlight = new LinearGradientBrush();
            highlight.StartPoint = new Point(0, 0);
            highlight.EndPoint = new Point(1, 1);
            highlight.GradientStops.Add(new GradientStop(ColorFrom(dark ? "#282D3930" : "#A8FFFFFF"), 0));
            highlight.GradientStops.Add(new GradientStop(ColorFrom(dark ? "#082D3930" : "#20FFFFFF"), 1));
            highlight.Freeze();
            resources["GlassHighlightBrush"] = highlight;
        }
    }

    private static void SetHighContrast(ResourceDictionary resources, bool useSystem)
    {
        var background = useSystem ? SystemColors.WindowColor : Colors.Black;
        var foreground = useSystem ? SystemColors.WindowTextColor : Colors.White;
        var accent = useSystem ? SystemColors.HighlightColor : Colors.Yellow;
        var accentText = useSystem ? SystemColors.HighlightTextColor : Colors.Black;
        foreach (var key in new[] { "WindowBackground", "SolidSurfaceBrush", "SurfaceBrush", "SubtleSurfaceBrush", "OverlayBrush", "GlassHighlightBrush", "WarmBrush", "AccentSoftBrush" })
            SetBrush(resources, key, background);
        foreach (var key in new[] { "TextPrimaryBrush", "TextSecondaryBrush", "BorderBrush" }) SetBrush(resources, key, foreground);
        SetBrush(resources, "AccentBrush", accent);
        SetBrush(resources, "SelectionBrush", accent);
        SetBrush(resources, "SelectionTextBrush", accentText);
        SetBrush(resources, "AccentTextBrush", accentText);
    }

    private static bool IsSystemLight()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return key?.GetValue("AppsUseLightTheme") is not int value || value != 0;
        }
        catch (System.Security.SecurityException) { return true; }
        catch (UnauthorizedAccessException) { return true; }
    }

    private static Color ColorFrom(string value) => (Color)ColorConverter.ConvertFromString(value);
    private static void SetBrush(ResourceDictionary resources, string key, string color) => SetBrush(resources, key, ColorFrom(color));
    private static void SetBrush(ResourceDictionary resources, string key, Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        resources[key] = brush;
    }

    private static void OnSystemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.HighContrast)) RefreshOnUiThread();
    }

    private static void RefreshOnUiThread()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.HasShutdownStarted)
            dispatcher.BeginInvoke(new Action(() => Apply(requestedTheme, reducedTransparency)));
    }
}
