using System.Reflection;

namespace NgMusic.Core;

public static class AppInfo
{
    public static string Version { get; } = ResolveVersion();

    private static string ResolveVersion()
    {
        var informational = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
            return informational.Split('+', 2)[0];

        return Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "dev";
    }
}
