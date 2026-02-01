# Qubic.Core

Core domain models, identity handling, transaction building, and signing abstractions for the Qubic network.

> **Note:** This is an early version. APIs may change in future releases.

## Install

```
dotnet add package Qubic.Core
```

## Features

- `QubicIdentity` value type (from seed, public key, or 60-character identity string)
- Transaction building and signing via `TransactionBuilder`
- Domain entities: `QubicTransaction`, `QubicBalance`, `QubicTick`, `QubicTransfer`, `QubicAsset`
- Protocol constants and well-known contract definitions
- Transfer, SendMany, and AssetTransfer payloads

## Usage

### Identity

```csharp
using Qubic.Core.Entities;

// From a 60-character identity string
var id = QubicIdentity.FromIdentity("BAAAAAAAA...");

// From a 55-character seed
var id = QubicIdentity.FromSeed("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc");

// From a 32-byte public key
var id = QubicIdentity.FromPublicKey(publicKeyBytes);
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

## Dependencies

- [Qubic.Crypto](https://www.nuget.org/packages/Qubic.Crypto)
