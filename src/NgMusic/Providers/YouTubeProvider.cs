using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using NgMusic.Auth;
using NgMusic.Core;

namespace NgMusic.Providers;

public sealed class YouTubeProvider(
    HttpClient http,
    GoogleCredentials credentials,
    IGoogleAuthService auth) : IMusicProvider
{
    public async Task<IReadOnlyList<MusicTrack>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var accessToken = await auth.GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken) && string.IsNullOrWhiteSpace(credentials.ApiKey))
        {
            throw new InvalidOperationException(
                "Search needs either an authenticated session ('login') or NGMUSIC_YOUTUBE_API_KEY.");
        }

        // Only return videos that YouTube says can be embedded and played
        // outside youtube.com. This prevents search results that are valid on
        // YouTube itself but fail immediately in the IFrame player.
        var url = "https://www.googleapis.com/youtube/v3/search" +
                  "?part=snippet&type=video&videoCategoryId=10" +
                  "&videoEmbeddable=true&videoSyndicated=true&maxResults=8" +
                  $"&q={Uri.EscapeDataString(query)}";

        if (string.IsNullOrWhiteSpace(accessToken))
            url += $"&key={Uri.EscapeDataString(credentials.ApiKey!)}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"YouTube API returned {(int)response.StatusCode}: {ReadApiError(body)}");

        using var doc = JsonDocument.Parse(body);
        var tracks = new List<MusicTrack>();
        foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
        {
            var id = item.GetProperty("id");
            if (!id.TryGetProperty("videoId", out var videoIdElement))
                continue;

            var snippet = item.GetProperty("snippet");
            tracks.Add(new MusicTrack(
                videoIdElement.GetString()!,
                WebUtility.HtmlDecode(snippet.GetProperty("title").GetString() ?? "Untitled"),
                WebUtility.HtmlDecode(snippet.GetProperty("channelTitle").GetString() ?? "Unknown")));
        }

        return tracks;
    }

    private static string ReadApiError(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("error").GetProperty("message").GetString() ?? "Unknown error";
        }
        catch
        {
            return "Unknown error";
        }
    }
}
