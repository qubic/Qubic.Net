# Qubic.Serialization

Binary serialization for the Qubic network protocol. Provides packet header structures, readers, and writers for encoding/decoding Qubic protocol messages.

> **Note:** This is an early version. APIs may change in future releases.

## Install

```
dotnet add package Qubic.Serialization
```

## Features

- `QubicPacketHeader` — 8-byte network packet header (type, size, dejavu)
- `QubicPacketReader` — deserializes binary protocol messages into domain entities
- `QubicPacketWriter` — serializes domain entities into binary protocol messages
- `QubicPacketTypes` — constants for all protocol message types

## Usage

```csharp
using Qubic.Serialization;

var writer = new QubicPacketWriter();
var reader = new QubicPacketReader();

// Write a request packet
byte[] packet = writer.WriteRequestCurrentTickInfo();

// Read a response packet
var header = reader.ReadHeader(responseBytes);
var tickInfo = reader.ReadCurrentTickInfo(responseBytes);
```

## Dependencies

- [Qubic.Core](https://www.nuget.org/packages/Qubic.Core)
