using System.Net.Sockets;
using System.Text;

namespace Cauce.Terminal.Infrastructure;

internal static class LoopbackHttp
{
    public static async Task<string> ReadTargetAsync(TcpClient client, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(
            client.GetStream(),
            Encoding.ASCII,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 4096,
            leaveOpen: true);

        var requestLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(requestLine))
            return "/";

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(line))
                break;
        }

        var parts = requestLine.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? parts[1] : "/";
    }

    public static async Task WriteResponseAsync(
        TcpClient client,
        string contentType,
        string content,
        int statusCode = 200,
        string statusText = "OK",
        CancellationToken cancellationToken = default)
    {
        var body = Encoding.UTF8.GetBytes(content);
        var header = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 {statusCode} {statusText}\r\n" +
            $"Content-Type: {contentType}\r\n" +
            $"Content-Length: {body.Length}\r\n" +
            "Cache-Control: no-store\r\n" +
            "Referrer-Policy: strict-origin-when-cross-origin\r\n" +
            "Connection: close\r\n\r\n");

        var stream = client.GetStream();
        await stream.WriteAsync(header, cancellationToken);
        await stream.WriteAsync(body, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }
}
