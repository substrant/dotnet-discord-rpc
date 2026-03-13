// UnixIpcTransport.cs
using System.Net.Sockets;

namespace Substrant.DotnetDiscordRPC.IPC;

/// <summary>
/// A Unix-specific implementation of <see cref="IIpcTransport"/> using Unix Domain Sockets.
/// </summary>
public sealed class UnixIpcTransport : IIpcTransport
{
    private Socket? _socket;
    private NetworkStream? _stream;

    /// <inheritdoc/>
    public async ValueTask ConnectAsync(CancellationToken ct = default)
    {
        // Discord checks these env vars in order to find the temp directory
        string? tmpDir =
            Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ??
            Environment.GetEnvironmentVariable("TMPDIR") ??
            Environment.GetEnvironmentVariable("TMP") ??
            Environment.GetEnvironmentVariable("TEMP") ??
            "/tmp";

        for (int i = 0; i <= 9; i++)
        {
            string path = Path.Combine(tmpDir, $"discord-ipc-{i}");
            if (!File.Exists(path)) continue;

            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                await socket.ConnectAsync(new UnixDomainSocketEndPoint(path), ct);
                _socket = socket;
                _stream = new NetworkStream(socket, ownsSocket: false);
                return;
            }
            catch
            {
                socket.Dispose();
            }
        }
        throw new IOException("Could not connect to Discord Unix socket. Ensure Discord is running.");
    }

    /// <inheritdoc/>
    public async ValueTask WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
        => await _stream!.WriteAsync(data, ct);

    /// <inheritdoc/>
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        => await _stream!.ReadAsync(buffer, ct);

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_stream is not null) await _stream.DisposeAsync();
        _socket?.Dispose(); // Ensure socket is also disposed
    }
}