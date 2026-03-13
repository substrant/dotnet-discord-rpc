using System.Buffers;
using System.Buffers.Binary;
using System.Text.Json;

using Substrant.DotnetDiscordRPC.IPC;
namespace Substrant.DotnetDiscordRPC;

/// <summary>
/// Opcodes used in the Discord IPC protocol.
/// </summary>
public enum Opcode : uint
{
    /// <summary>Initial handshake frame.</summary>
    Handshake = 0,
    /// <summary>Standard data frame.</summary>
    Frame = 1,
    /// <summary>Close connection frame.</summary>
    Close = 2,
    /// <summary>Ping frame.</summary>
    Ping = 3,
    /// <summary>Pong frame.</summary>
    Pong = 4,
}

/// <summary>
/// A high-performance, AOT-compatible Discord Rich Presence client.
/// </summary>
public sealed class DiscordRpcClient : IAsyncDisposable
{
    private readonly IIpcTransport _transport;
    private readonly string _clientId;
    private int _nonce;

    /// <summary>
    /// Invoked when a message is received from Discord (Opcode + JSON string).
    /// </summary>
    public event Action<Opcode, string>? OnMessage;

    /// <summary>
    /// Invoked when the client is ready and connected.
    /// </summary>
    public event Action? OnReady;
    
    /// <summary>
    /// Invoked when the client connection is disposed.
    /// </summary>
    public event Action? OnDisposed;
    
    /// <summary>
    /// Invoked when the RPC activity is updated.
    /// </summary>
    public event Action? OnRpcUpdated;

    /// <summary>
    /// Gets whether the client is currently connected to Discord IPC.
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscordRpcClient"/> class.
    /// </summary>
    /// <param name="clientId">Your Discord Application Client ID.</param>
    public DiscordRpcClient(string clientId)
    {
        _clientId = clientId;
        _transport = OperatingSystem.IsWindows()
            ? new WindowsIpcTransport()
            : new UnixIpcTransport();
    }

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Connects to the Discord IPC backend and performs the initial handshake.
    /// </summary>
    /// <param name="ct">Token to cancel the connection process.</param>
    /// <returns>A ValueTask that completes when connected.</returns>
    public async ValueTask ConnectAsync(CancellationToken ct = default)
    {
        await _transport.ConnectAsync(ct);
        await SendAsync(Opcode.Handshake, new Handshake(1, _clientId), DiscordJsonContext.Default.Handshake, ct);
        // Read and discard the READY response
        await ReceiveAsync(ct);
        IsConnected = true;
        OnReady?.Invoke();
    }

    /// <summary>
    /// Updates the user's Rich Presence activity.
    /// </summary>
    /// <param name="activity">The new activity to display.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public async ValueTask SetActivityAsync(Activity activity, CancellationToken ct = default)
    {
        var payload = new RpcPayload(
            Command: "SET_ACTIVITY",
            Args: new RpcArgs(Pid: Environment.ProcessId, Activity: activity),
            Nonce: (++_nonce).ToString()
        );
        await SendAsync(Opcode.Frame, payload, DiscordJsonContext.Default.RpcPayload, ct);
        await ReceiveAsync(ct); // ack
        OnRpcUpdated?.Invoke();
    }

    /// <summary>
    /// Clears the current Rich Presence activity.
    /// </summary>
    /// <param name="ct">Token to cancel the operation.</param>
    public async ValueTask ClearActivityAsync(CancellationToken ct = default)
    {
        var payload = new RpcPayload(
            Command: "SET_ACTIVITY",
            Args: new RpcArgs(Pid: Environment.ProcessId, Activity: null),
            Nonce: (++_nonce).ToString()
        );
        await SendAsync(Opcode.Frame, payload, DiscordJsonContext.Default.RpcPayload, ct);
        await ReceiveAsync(ct);
        OnRpcUpdated?.Invoke();
    }

    // ── Framing ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Serializes and sends a payload to the IPC transport.
    /// </summary>
    /// <typeparam name="T">The type of the payload.</typeparam>
    /// <param name="opcode">The opcode for the frame.</param>
    /// <param name="payload">The data object to serialize.</param>
    /// <param name="typeInfo">JSON type info for AOT serialization.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    private async ValueTask SendAsync<T>(Opcode opcode, T payload, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo, CancellationToken ct)
    {
        if (!IsConnected && opcode != Opcode.Handshake)
            throw new InvalidOperationException("Not connected to Discord.");

        // Serialize with AOT-safe context
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(payload, typeInfo);

        // 8-byte header + json body
        int totalLen = 8 + json.Length;
        byte[] buf = ArrayPool<byte>.Shared.Rent(totalLen);
        try
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(0, 4), (uint)opcode);
            BinaryPrimitives.WriteUInt32LittleEndian(buf.AsSpan(4, 4), (uint)json.Length);
            json.CopyTo(buf.AsSpan(8));
            await _transport.WriteAsync(buf.AsMemory(0, totalLen), ct);
        }
        catch
        {
            IsConnected = false;
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
    }

    /// <summary>
    /// Reads and parses a frame from the IPC transport.
    /// </summary>
    /// <param name="ct">Token to cancel the operation.</param>
    /// <returns>A tuple containing the opcode and the raw JSON body bytes.</returns>
    private async ValueTask<(Opcode opcode, byte[] data)> ReceiveAsync(CancellationToken ct)
    {
        // Read exactly 8 header bytes
        byte[] header = new byte[8];
        await ReadExactAsync(header, ct);

        var opcode = (Opcode)BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(0, 4));
        int length = (int)BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4, 4));

        byte[] body = new byte[length];
        await ReadExactAsync(body, ct);

        if (OnMessage != null)
        {
            try
            {
                var json = System.Text.Encoding.UTF8.GetString(body);
                OnMessage?.Invoke(opcode, json);
            }
            catch { }
        }

        return (opcode, body);
    }

    /// <summary>Reads exactly buffer.Length bytes — required because pipes/sockets can short-read.</summary>
    private async ValueTask ReadExactAsync(byte[] buffer, CancellationToken ct)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = await _transport.ReadAsync(buffer.AsMemory(offset), ct);
            if (read == 0) throw new EndOfStreamException("Discord IPC connection closed.");
            offset += read;
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        OnDisposed?.Invoke();
        return _transport.DisposeAsync();
    }
}