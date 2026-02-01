# Qubic.Net

A modular .NET 8.0 library for interacting with the [Qubic](https://qubic.org) network. Provides direct TCP node communication, HTTP RPC, and QubicBob JSON-RPC clients, along with core domain models, binary serialization, and pure C# cryptographic primitives.

## Packages

| Package | Description |
|---------|-------------|
| **Qubic.Crypto** | Pure C# implementation of Qubic cryptographic primitives: K12 (KangarooTwelve) hashing, FourQ elliptic curve, SchnorrQ signatures, and ECDH key exchange. Zero external dependencies. |
| **Qubic.Core** | Core domain models, identity handling, transaction building, and signing abstractions. |
| **Qubic.Serialization** | Binary serialization for the Qubic network protocol (packet headers, readers, writers). |
| **Qubic.Network** | Direct TCP client for communicating with Qubic network nodes. |
| **Qubic.Rpc** | HTTP client for the official Qubic RPC API. |
| **Qubic.Bob** | JSON-RPC client for the QubicBob API. |

### Dependency Graph

```
Qubic.Crypto        (no dependencies)
  └── Qubic.Core
       └── Qubic.Serialization
            └── Qubic.Network
       └── Qubic.Rpc
       └── Qubic.Bob
```

## Usage

### Identity

```csharp
using Qubic.Core.Entities;

// From a 60-character identity string
var identity = QubicIdentity.FromIdentity("BAAAAAAAA...");

// From a 55-character seed
var identity = QubicIdentity.FromSeed("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc");

// From a 32-byte public key
var identity = QubicIdentity.FromPublicKey(publicKeyBytes);
```

### Build and Sign a Transaction

```csharp
using Qubic.Core;
using Qubic.Core.Entities;

var builder = new TransactionBuilder();

var tx = builder.CreateTransfer(
    source: QubicIdentity.FromSeed(seed),
    destination: QubicIdentity.FromIdentity("DEST..."),
    amount: 1000,
    tick: currentTick + 5
);

builder.Sign(tx, seed);
```

### Direct TCP Node Communication

```csharp
using Qubic.Network;

using var client = new QubicNodeClient("164.90.178.16");
await client.ConnectAsync();

var tickInfo = await client.GetCurrentTickInfoAsync();
Console.WriteLine($"Tick: {tickInfo.Tick}, Epoch: {tickInfo.Epoch}");

var balance = await client.GetBalanceAsync(identity);
Console.WriteLine($"Balance: {balance.Amount}");

await client.BroadcastTransactionAsync(signedTx);
```

### HTTP RPC

```csharp
using Qubic.Rpc;

using var rpc = new QubicRpcClient("https://rpc.qubic.org");

var tick = await rpc.GetLatestTickAsync();
var balance = await rpc.GetBalanceAsync(identity);
var txId = await rpc.BroadcastTransactionAsync(signedTx);
```

### QubicBob JSON-RPC

```csharp
using Qubic.Bob;

using var bob = new BobClient("http://localhost:40420");

var chainId = await bob.GetChainIdAsync();
var tickNumber = await bob.GetTickNumberAsync();
var balance = await bob.GetBalanceAsync(identity);
var transfers = await bob.GetTransfersAsync(identity, startTick: 1000, endTick: 2000);
```

## Building

```bash
dotnet build
```

## Testing

```bash
dotnet test
```

Real-node integration tests require the `QUBIC_NODE_IP` environment variable:

```bash
QUBIC_NODE_IP=164.90.178.16 dotnet test
```

