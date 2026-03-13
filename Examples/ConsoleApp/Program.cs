using Substrant.DotnetDiscordRPC;

Console.Title = "Discord RPC AOT Demo";
Console.WriteLine("Dotnet Discord RPC Example (AOT)");

// 1. Create client instance (Use your own Client ID from Discord Developer Portal)
string clientId = "123456789012345678"; // Replace with your actual Client ID
if (args.Length > 0) clientId = args[0];

await using var client = new DiscordRpcClient(clientId);

// 2. Setup event handlers
client.OnReady += () => Console.WriteLine("[Event] Client is Ready!");
client.OnRpcUpdated += () => Console.WriteLine("[Event] Presence Updated!");
client.OnDisposed += () => Console.WriteLine("[Event] Client Disposed.");
client.OnMessage += (op, msg) => Console.WriteLine($"[IPC] {op}: {msg}"); // Debug logging

Console.Write("Connecting... ");
try
{
    // 3. Connect
    await client.ConnectAsync();
    Console.WriteLine("Connected!");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed: {ex.Message}");
    return;
}

// 4. Create an activity object
// Make sure you have uploaded assets with these keys to your app's Rich Presence Assets
var activity = new Activity(
    Details: "Testing .NET 10 AOT DiscordRPC",
    State: "In the Console",
    Timestamps: new Timestamps(
        Start: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        End: null
    ),
    Assets: new Assets(
        LargeImage: "some_large_image_name",
        LargeText: ".NET 10 Framework",
        SmallImage: "some_small_image_name",
        SmallText: "Some Small Text"
    ),
    Buttons: [
        new Button("View Code", "https://github.com/dotnet/core"),
        new Button("Download .NET", "https://get.dotnet.microsoft.com/")
    ],
    Instance: true
);

// 5. Update Presence
Console.WriteLine("Setting Activity...");
await SafeSetActivity(client, activity);

Console.WriteLine("\nControls:");
Console.WriteLine("  [C] Clear Activity");
Console.WriteLine("  [U] Update Details");
Console.WriteLine("  [R] Reset Activity");
Console.WriteLine("  [Q] Quit");
Console.WriteLine("  [X] Reconnect (Experimental)");

// Main loop
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => {
    e.Cancel = true;
    cts.Cancel();
};

async ValueTask SafeSetActivity(DiscordRpcClient client, Activity activity, CancellationToken ct = default)
{
    try 
    {
        await client.SetActivityAsync(activity, ct);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Failed to set activity: {ex.Message}");
    }
}

try 
{
    while (!cts.Token.IsCancellationRequested)
    {
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.C:
                    Console.WriteLine("Clearing Activity...");
                    try { await client.ClearActivityAsync(cts.Token); } catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
                    break;
                case ConsoleKey.U:
                    Console.WriteLine("Updating Timestamp...");
                    // Using 'with' expression for immutable record update
                    activity = activity with { 
                        Details = $"Updated at {DateTime.Now:HH:mm:ss}",
                        Timestamps = new Timestamps(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), null)
                    };
                    await SafeSetActivity(client, activity, cts.Token);
                    break;
                case ConsoleKey.R:
                    Console.WriteLine("Resetting Activity...");
                    await SafeSetActivity(client, activity, cts.Token);
                    break;
                case ConsoleKey.X:
                     Console.WriteLine("Reconnecting...");
                     try {
                         if (!client.IsConnected) await client.ConnectAsync(cts.Token);
                         else Console.WriteLine("Already connected.");
                     }
                     catch (Exception ex) { Console.WriteLine($"Reconnect failed: {ex.Message}"); }
                     break;
                case ConsoleKey.Q:
                    cts.Cancel();
                    break;
            }
        }
        await Task.Delay(100, cts.Token);
    }
}
catch (OperationCanceledException) { }

Console.WriteLine("Exiting...");
// DisposeAsync is called automatically at end of scope due to 'await using'
