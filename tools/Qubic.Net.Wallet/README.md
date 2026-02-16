# Qubic.Net Wallet

Cross-platform desktop wallet for the [Qubic](https://qubic.org) network. Runs as a native desktop window via [Photino.Blazor](https://github.com/AhLamm/photino.Blazor), or as a Blazor Server app in the browser with `--server`.

## Features

- **Dashboard** — identity, QU balance, network status, owned assets, and open orders at a glance
- **Send / Receive** — single transfers and batch "Send to Many"
- **Assets** — view and transfer QX-listed tokens
- **QX Trading** — place and cancel asset orders on the Qubic DEX
- **Qearn Staking** — lock/unlock QU for staking rewards
- **Qswap DEX** — decentralized token swap interface
- **MSVault** — multi-signature vault management (create, deposit, release, approve/deny)
- **Transaction History** — full history with local encrypted storage and sync
- **Sign / Verify** — sign messages with your seed and verify signatures
- **Settings** — backend selection (RPC / Bob / Direct TCP), peer management, label sources, database import/export

## Quick Start

```bash
# Desktop mode (native window)
dotnet run --project tools/Qubic.Net.Wallet

# Server mode (opens in browser)
dotnet run --project tools/Qubic.Net.Wallet -- --server
```

## Security

- Your 55-character seed is held **in memory only** for the current session — it is never written to disk
- The local database is encrypted with [SQLCipher](https://www.zetetic.net/sqlcipher/) using a key derived from your seed
- In server mode, access is protected by a one-time session token (HttpOnly cookie, localhost only)
- The database file is useless without the seed

## Release Builds

```bash
# Build self-contained single-file binaries for all platforms
./tools/Qubic.Net.Wallet/publish.sh
```

Produces zip archives with SHA-256 hashes for:
- `win-x64`
- `osx-x64`
- `osx-arm64`
- `linux-x64`

## Architecture

- **UI**: Blazor components with Bootstrap 5 dark theme
- **Desktop**: Photino.Blazor (WebView2 on Windows, WebKit on macOS/Linux)
- **Server fallback**: Blazor Server with auto-launched browser (`--server` flag, or automatic fallback if native libraries are unavailable)
- **Storage**: SQLCipher-encrypted SQLite per identity
- **Backends**: RPC, Bob, or direct TCP to Qubic nodes
