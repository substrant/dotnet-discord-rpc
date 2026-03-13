# Substrant.DotnetDiscordRPC

**AOT-compatible, cross-platform, high-performance Discord Rich Presence library for .NET.**  
Maintained by **Substrant Softworks LLC**.

`Substrant.DotnetDiscordRPC` provides a lightweight implementation of the Discord IPC protocol designed for modern .NET applications. It works with **Native AOT**, **.NET 8+**, and supports **Windows, Linux, and macOS**.

---

# Overview

Discord Rich Presence allows applications to display detailed activity information in a user's Discord profile.

This library focuses on:

- Native AOT compatibility
- Minimal allocations
- Async-first API
- Cross-platform IPC support
- Simple and modern .NET design

It communicates directly with Discord's **local IPC socket** and provides a clean, developer-friendly API.

---

# Features

- ⚡ High performance
- 🧵 Async / await based
- 🧩 Native AOT compatible
- 🌍 Cross-platform (Windows / Linux / macOS)
- 🪶 Lightweight implementation
- 📦 Minimal dependencies
- 🧱 Immutable activity models
- 📡 Event-driven architecture

---

# Installation

Install via **NuGet**:
`dotnet add package Substrant.DotnetDiscordRPC`

Or install through the NuGet Package Manager in Visual Studio.

---

# API Documentation

## DiscordRpcClient

The main entry point for interacting with Discord Rich Presence.

### Constructor

```csharp
DiscordRpcClient(string clientId)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| clientId | string | Your Discord Application Client ID |

---

### Properties

| Property | Type | Description |
|--------|------|-------------|
| IsConnected | bool | Indicates whether the IPC connection to Discord is active |

---

### Methods

#### ConnectAsync

Connects to the Discord IPC socket.

```csharp
Task ConnectAsync(CancellationToken cancellationToken = default)
```

**Example**

```csharp
await client.ConnectAsync();
```

---

#### SetActivityAsync

Sets the user's Discord Rich Presence activity.

```csharp
Task SetActivityAsync(Activity activity, CancellationToken cancellationToken = default)
```

| Parameter | Type | Description |
|-----------|------|-------------|
| activity | Activity | Activity to display |

---

#### ClearActivityAsync

Clears the current activity.

```csharp
Task ClearActivityAsync(CancellationToken cancellationToken = default)
```

---

#### DisposeAsync

Disposes the client and closes the IPC connection.

```csharp
ValueTask DisposeAsync()
```

---

## Events

### OnReady

Triggered when the connection to Discord is established.

```csharp
event Action OnReady
```

Example:

```csharp
client.OnReady += () => Console.WriteLine("RPC Ready!");
```

---

### OnRpcUpdated

Triggered when the Rich Presence is successfully updated.

```csharp
event Action OnRpcUpdated
```

---

### OnDisposed

Triggered when the client is disposed.

```csharp
event Action OnDisposed
```

---

### OnMessage

Triggered when a raw IPC message is received (useful for debugging).

```csharp
event Action<int, string> OnMessage
```

| Parameter | Description |
|----------|-------------|
| op | Discord IPC opcode |
| message | Raw message payload |

---

# Activity

Represents a Discord Rich Presence activity.

### Constructor

```csharp
Activity(
    string? Details = null,
    string? State = null,
    Timestamps? Timestamps = null,
    Assets? Assets = null,
    IReadOnlyList<Button>? Buttons = null,
    bool Instance = false
)
```

---

### Properties

| Property | Type | Description |
|--------|------|-------------|
| Details | string? | Main activity description |
| State | string? | Secondary activity text |
| Timestamps | Timestamps? | Start / end timestamps |
| Assets | Assets? | Rich Presence images |
| Buttons | IReadOnlyList<Button>? | Activity buttons |
| Instance | bool | Whether the activity is an instance |

---

# Timestamps

Represents activity timestamps.

### Constructor

```csharp
Timestamps(long? Start = null, long? End = null)
```

| Property | Type | Description |
|--------|------|-------------|
| Start | long? | Unix timestamp when activity started |
| End | long? | Unix timestamp when activity ends |

---

# Assets

Represents Rich Presence image assets.

### Constructor

```csharp
Assets(
    string? LargeImage = null,
    string? LargeText = null,
    string? SmallImage = null,
    string? SmallText = null
)
```

| Property | Type | Description |
|--------|------|-------------|
| LargeImage | string? | Large asset key |
| LargeText | string? | Hover text for large image |
| SmallImage | string? | Small asset key |
| SmallText | string? | Hover text for small image |

Assets must be uploaded in the **Discord Developer Portal**.

---

# Button

Represents an activity button.

### Constructor

```csharp
Button(string Label, string Url)
```

| Property | Type | Description |
|--------|------|-------------|
| Label | string | Button text |
| Url | string | Button URL |

Discord allows **up to 2 buttons per activity**.

---

# Notes

- Discord must be running locally for IPC to work.
- Rich Presence assets must be uploaded in the Discord application dashboard.
- Discord limits activity updates to prevent spam.

More details:
https://discord.com/developers/docs/rich-presence/how-to

---

# Events

The client exposes several events for monitoring state.

| Event | Description |
|------|-------------|
| `OnReady` | Fired when the RPC connection is ready |
| `OnRpcUpdated` | Fired when the activity updates successfully |
| `OnDisposed` | Fired when the client is disposed |

Example:
```
await using var client = new DiscordRpcClient(clientId);
client.OnReady += () => Console.WriteLine("Connected!");
client.OnRpcUpdated += () => Console.WriteLine("Presence updated!");
```

---

# Activity Structure

Rich Presence activities are composed of several components:

- **Details** – Main activity text  
- **State** – Secondary description  
- **Timestamps** – Start / end times  
- **Assets** – Large and small images  
- **Buttons** – Up to 2 clickable buttons  
- **Instance** – Whether the activity is joinable  

Assets must be uploaded in your **Discord Developer Portal** application.

---

# AOT Compatibility

This library is designed to work with **Native AOT builds** without requiring reflection.

Example publish command:
`dotnet publish -c Release -r win-x64 -p:PublishAot=true`

---

# Requirements

- .NET 8 or newer
- Discord Desktop client running

---

# License

Apache 2.0 License

---

# Maintained By

**Substrant Softworks LLC**
