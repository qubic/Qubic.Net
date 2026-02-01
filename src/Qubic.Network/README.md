# Qubic.Network

Direct TCP client for communicating with Qubic network nodes. Connects to nodes using the native Qubic binary protocol.

> **Note:** This is an early version. APIs may change in future releases.

## Install

```
dotnet add package Qubic.Network
```

## Features

- Direct TCP connection to Qubic nodes (default port 21841)
- Query tick info, balances, and entity data
- Broadcast signed transactions
- Peer discovery

## Usage

```csharp
using Qubic.Network;
using Qubic.Core.Entities;

using var client = new QubicNodeClient("164.90.178.16");
await client.ConnectAsync();

// Get current tick info
var tickInfo = await client.GetCurrentTickInfoAsync();
Console.WriteLine($"Tick: {tickInfo.Tick}, Epoch: {tickInfo.Epoch}");

// Query balance
var identity = QubicIdentity.FromIdentity("BAAAAAAAA...");
var balance = await client.GetBalanceAsync(identity);
Console.WriteLine($"Balance: {balance.Amount}");

// Broadcast a signed transaction
await client.BroadcastTransactionAsync(signedTx);
```

## Dependencies

- [Qubic.Serialization](https://www.nuget.org/packages/Qubic.Serialization)
