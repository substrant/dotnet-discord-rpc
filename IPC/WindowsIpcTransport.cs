// WindowsIpcTransport.cs
using System.IO.Pipes;

namespace Substrant.DotnetDiscordRPC.IPC;

/// <summary>
/// A Windows-specific implementation of <see cref="IIpcTransport"/> using Named Pipes.
/// </summary>
public sealed class WindowsIpcTransport : IIpcTransport
{
    private NamedPipeClientStream? _pipe;

    /// <inheritdoc/>
    public async ValueTask ConnectAsync(CancellationToken ct = default)
    {
        for (int i = 0; i <= 9; i++)
        {
            // Windows utilizes named pipes for IPC
            var pipe = new NamedPipeClientStream(
                ".",
                $"discord-ipc-{i}",
                PipeDirection.InOut,
                PipeOptions.Asynchronous
            );

            try
            {
                await pipe.ConnectAsync(timeout: 500, ct);
                _pipe = pipe;
                return;
            }
            catch
            {
                await pipe.DisposeAsync();
            }
        }
        throw new IOException("Could not connect to Discord IPC pipe. Ensure Discord is running.");
    }

    /// <inheritdoc/>
    public async ValueTask WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
        => await _pipe!.WriteAsync(data, ct);

    /// <inheritdoc/>
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        => await _pipe!.ReadAsync(buffer, ct);

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        _pipe?.Dispose();
        return ValueTask.CompletedTask;
    }
}