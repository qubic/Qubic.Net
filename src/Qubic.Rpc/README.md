# Qubic.Rpc

HTTP client for the official Qubic RPC API. Covers both the Live API (current network state) and the Query API (historical/archive data).

> **Note:** This is an early version. APIs may change in future releases.

## Install

```
dotnet add package Qubic.Rpc
```

## Features

- **Live API** (`/live/v1`) — current tick info, balances, transaction broadcasting, smart contract queries
- **Query API** (`/query/v1`) — historical transactions, tick data, asset records, computor lists
- Returns typed domain entities from `Qubic.Core`
- Supports custom `HttpClient` injection

## Usage

```csharp
using Qubic.Rpc;
using Qubic.Core.Entities;

using var rpc = new QubicRpcClient("https://rpc.qubic.org");

// Live network state
var tickInfo = await rpc.GetTickInfoAsync();
var balance = await rpc.GetBalanceAsync(QubicIdentity.FromIdentity("BAAAAAAAA..."));

// Broadcast a signed transaction
var txId = await rpc.BroadcastTransactionAsync(signedTx);

// Query historical data
var latestTick = await rpc.GetLatestTickAsync();
var transfers = await rpc.GetTransactionsForIdentityAsync(identity, startTick, endTick);
```

## Dependencies

- [Qubic.Core](https://www.nuget.org/packages/Qubic.Core)
