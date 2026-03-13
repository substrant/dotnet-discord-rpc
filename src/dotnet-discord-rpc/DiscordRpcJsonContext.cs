// Models.cs
using System.Text.Json.Serialization;

namespace Substrant.DotnetDiscordRPC;

/// <summary>
/// Represents the initial handshake payload sent to Discord.
/// </summary>
/// <param name="Version">The IPC version (usually 1).</param>
/// <param name="ClientId">The Discord Application Client ID.</param>
public record Handshake(
    [property: JsonPropertyName("v")] int Version,
    [property: JsonPropertyName("client_id")] string ClientId
);

/// <summary>
/// Represents the envelope for sending commands to Discord IPC.
/// </summary>
/// <param name="Command">The command to execute (e.g., "SET_ACTIVITY").</param>
/// <param name="Args">The arguments associated with the command.</param>
/// <param name="Nonce">A unique string to identify the request/response pair.</param>
public record RpcPayload(
    [property: JsonPropertyName("cmd")] string Command,
    [property: JsonPropertyName("args")] RpcArgs? Args,
    [property: JsonPropertyName("nonce")] string Nonce
);

/// <summary>
/// Arguments for an RPC command.
/// </summary>
/// <param name="Pid">The process ID of the application.</param>
/// <param name="Activity">The rich presence activity payload.</param>
public record RpcArgs(
    [property: JsonPropertyName("pid")] int Pid,
    [property: JsonPropertyName("activity")] Activity? Activity
);

/// <summary>
/// Defines the structure of the Rich Presence activity.
/// </summary>
/// <param name="Details">What the player is currently doing.</param>
/// <param name="State">The user's current party status.</param>
/// <param name="Timestamps">Unix timestamps for start and/or end of the game.</param>
/// <param name="Assets">Images and text for the large and small assets.</param>
/// <param name="Buttons">Array of buttons (label and url).</param>
/// <param name="Instance">Whether this is an instance of a game session.</param>
public record Activity(
    [property: JsonPropertyName("details")] string? Details,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("timestamps")] Timestamps? Timestamps,
    [property: JsonPropertyName("assets")] Assets? Assets,
    [property: JsonPropertyName("buttons")] Button[]? Buttons = null,
    [property: JsonPropertyName("instance")] bool Instance = true
);

/// <summary>
/// Represents a button in the Rich Presence activity.
/// </summary>
/// <param name="Label">Text displayed on the button.</param>
/// <param name="Url">URL opened when the button is clicked.</param>
public record Button(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("url")] string Url
);

/// <summary>
/// Timestamps for the activity duration.
/// </summary>
/// <param name="Start">Unix timestamp (seconds) for the start of the activity.</param>
/// <param name="End">Unix timestamp (seconds) for the end of the activity.</param>
public record Timestamps(
    [property: JsonPropertyName("start")] long? Start,
    [property: JsonPropertyName("end")] long? End
);

/// <summary>
/// Asset keys and text for the rich presence images.
/// </summary>
/// <param name="LargeImage">Key for the large image asset.</param>
/// <param name="LargeText">Tooltip text for the large image.</param>
/// <param name="SmallImage">Key for the small image asset.</param>
/// <param name="SmallText">Tooltip text for the small image.</param>
public record Assets(
    [property: JsonPropertyName("large_image")] string? LargeImage,
    [property: JsonPropertyName("large_text")] string? LargeText,
    [property: JsonPropertyName("small_image")] string? SmallImage,
    [property: JsonPropertyName("small_text")] string? SmallText
);

// AOT source-generated serialization context — NO reflection at runtime
/// <summary>
/// Source-generated JSON context for AOT and trimming compatibility.
/// </summary>
[JsonSerializable(typeof(Handshake))]
[JsonSerializable(typeof(RpcPayload))]
[JsonSerializable(typeof(RpcArgs))]
[JsonSerializable(typeof(Activity))]
[JsonSerializable(typeof(Button))]
[JsonSerializable(typeof(Button[]))]
[JsonSerializable(typeof(Timestamps))]
[JsonSerializable(typeof(Assets))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, 
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
)]
public partial class DiscordJsonContext : JsonSerializerContext { }