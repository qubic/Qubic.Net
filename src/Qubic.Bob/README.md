# Qubic.Bob

JSON-RPC client for the [QubicBob](https://github.com/qubic/qubic-bob) API. Provides both an HTTP client (`BobClient`) and a WebSocket client (`BobWebSocketClient`) with real-time subscriptions, multi-node failover, and automatic reconnection.

> **Note:** This is an early version. APIs may change in future releases.

## Install

```
dotnet add package Qubic.Bob
```

## Features

- **BobClient** — HTTP JSON-RPC client for simple request/response queries
- **BobWebSocketClient** — persistent WebSocket connection with:
  - Multi-node failover and health monitoring
  - Automatic reconnection with exponential backoff
  - Managed subscriptions that survive reconnects
  - Both RPC queries and real-time subscriptions over a single connection
- Tick stream, new-tick, transfer, and log subscriptions via `IAsyncEnumerable`
- 15+ convenience RPC methods (balance, ticks, transactions, epochs, computors, etc.)

## Usage

### HTTP Client

```csharp
using Qubic.Bob;

using var bob = new BobClient("http://localhost:40420");

var chainId = await bob.GetChainIdAsync();
var tick = await bob.GetTickNumberAsync();
var balance = await bob.GetBalanceAsync(identity);
```

### WebSocket Client

```csharp
using Qubic.Bob;

var options = new BobWebSocketOptions
{
    Nodes = ["https://bob01.qubic.li", "https://bob02.qubic.li"]
};

await using var client = new BobWebSocketClient(options);
await client.ConnectAsync();

// RPC queries
var balance = await client.GetBalanceAsync("IDENTITY...");
var epoch = await client.GetCurrentEpochAsync();
var tick = await client.GetTickNumberAsync();

// Real-time subscriptions
using var ticks = await client.SubscribeNewTicksAsync();
await foreach (var tick in ticks)
{
    Console.WriteLine($"New tick: {tick.Tick}");
}
```

### Tick Stream (full transaction + log data)

```csharp
var streamOptions = new TickStreamOptions
{
    StartTick = 20000000,
    IncludeInputData = true
};

using var stream = await client.SubscribeTickStreamAsync(streamOptions);
await foreach (var notification in stream)
{
    Console.WriteLine($"Tick {notification.Tick}: {notification.TxCountTotal} txs, {notification.LogCountTotal} logs");
}
```

## Dependencies

- [Qubic.Core](https://www.nuget.org/packages/Qubic.Core)
