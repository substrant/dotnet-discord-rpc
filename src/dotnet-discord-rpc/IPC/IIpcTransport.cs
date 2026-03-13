// IpcTransport.cs
namespace Substrant.DotnetDiscordRPC.IPC;

/// <summary>
/// Represents the low-level transport layer for communicating with the Discord IPC server.
/// Implementations handle platform-specific socket or pipe connections.
/// </summary>
public interface IIpcTransport : IAsyncDisposable
{
    /// <summary>
    /// Establishes a connection to the local Discord IPC instance.
    /// </summary>
    /// <param name="ct">Token to cancel the connection attempt.</param>
    /// <returns>A ValueTask that completes when the connection is established.</returns>
    ValueTask ConnectAsync(CancellationToken ct = default);

    /// <summary>
    /// Writes a sequence of bytes to the underlying transport stream.
    /// </summary>
    /// <param name="data">The buffer containing the data to send.</param>
    /// <param name="ct">Token to cancel the write operation.</param>
    /// <returns>A ValueTask that completes when the write operation finishes.</returns>
    ValueTask WriteAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default);

    /// <summary>
    /// Reads data from the underlying transport stream into the provided buffer.
    /// </summary>
    /// <param name="buffer">The buffer to write the received data into.</param>
    /// <param name="ct">Token to cancel the read operation.</param>
    /// <returns>The number of bytes read.</returns>
    ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default);
}