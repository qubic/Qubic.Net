# Qubic.Crypto

Pure C#/.NET implementation of Qubic cryptographic primitives. Zero external dependencies.

> **Note:** This is an early version. APIs may change in future releases.

## Features

- **K12 (KangarooTwelve)** hashing
- **FourQ** elliptic curve operations
- **SchnorrQ** digital signatures
- **ECDH** key exchange
- Identity derivation (seed to public key to 60-character identity)

## Install

```
dotnet add package Qubic.Crypto
```

## Usage

```csharp
using Qubic.Crypto;

var crypto = new QubicCrypt();

// Derive keys from a 55-character seed
string seed = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc";
byte[] privateKey = crypto.GetPrivateKey(seed);
byte[] publicKey = crypto.GetPublicKey(seed);

// Get the 60-character Qubic identity
string identity = crypto.GetIdentityFromPublicKey(publicKey);

// Hash data with KangarooTwelve
byte[] hash = crypto.KangarooTwelve(data);

// Sign and verify
byte[] signed = crypto.Sign(seed, message);
bool valid = crypto.Verify(publicKey, message, signature);

// ECDH shared key
byte[] sharedKey = crypto.GetSharedKey(seed, peerPublicKey);
```

## License

MIT
