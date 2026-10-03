using System.Globalization;
using System.Text;

namespace Cauce.Core;

/// <summary>Deterministic selection; history is ordered from oldest to newest and stays in memory.</summary>
public sealed class QueuePlanner
{
    public QueueChoice Next(IReadOnlyList<Track> library, SessionRules rules, IReadOnlyList<string> history)
    {
        ArgumentNullException.ThrowIfNull(library);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(history);
        ArgumentOutOfRangeException.ThrowIfNegative(rules.ArtistSpacing);

        var available = library.Where(t => t.Source == TrackSource.LocalFile && t.IsAvailable).ToList();
        if (available.Count == 0)
            return new(null, "No hay archivos locales disponibles. Importa música o revisa su ubicación.");

        var genre = Normalize(rules.Genre);
        if (genre.Length != 0 && genre != "TODOS")
            available = available.Where(t => Normalize(t.Genre) == genre).ToList();
        if (available.Count == 0)
            return new(null, "No hay canciones disponibles del género elegido. Cambia el género o añade música.");

        var played = new HashSet<string>(history, StringComparer.Ordinal);
        if (!rules.AllowRepeats)
            available = available.Where(t => !played.Contains(t.Id)).ToList();
        if (available.Count == 0)
            return new(null, "Ya escuchaste todas las canciones que cumplen el género. Permite repeticiones o inicia otra sesión.");

        var tracksById = library.GroupBy(t => t.Id, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var recentArtists = new HashSet<string>(StringComparer.Ordinal);
        for (var i = Math.Max(0, history.Count - rules.ArtistSpacing); i < history.Count; i++)
        {
            if (tracksById.TryGetValue(history[i], out var track))
            {
                var artist = Normalize(track.Artist);
                // Missing metadata cannot establish that two files share an artist.
                if (artist.Length > 0) recentArtists.Add(artist);
            }
        }

        var candidates = available.Where(t => !recentArtists.Contains(Normalize(t.Artist))).ToList();
        if (candidates.Count == 0)
            return new(null, "La separación entre artistas impide continuar. Añade otros artistas o ajusta esa regla; no la cambiamos automáticamente.");

        // Prefer unheard tracks, then the least recently played; stable library order breaks ties.
        var lastPlayed = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < history.Count; i++) lastPlayed[history[i]] = i;
        var next = candidates.MinBy(t => lastPlayed.GetValueOrDefault(t.Id, -1))!;
        var reason = genre.Length == 0 || genre == "TODOS"
            ? "Cumple las reglas de esta sesión."
            : $"Coincide con el género {rules.Genre.Trim()} y cumple las reglas de esta sesión.";
        if (string.IsNullOrWhiteSpace(next.Artist))
            reason += " Falta el artista; completa ese dato para poder aplicar su separación.";
        return new(next, reason);
    }

    internal static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var result = new StringBuilder();
        var space = false;
        foreach (var c in value.Trim().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsWhiteSpace(c)) { space = result.Length > 0; continue; }
            if (space) result.Append(' ');
            space = false;
            result.Append(char.ToUpperInvariant(c));
        }
        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
