#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PROJECT="$SCRIPT_DIR/Qubic.Toolkit.csproj"
PUBLISH_DIR="$SCRIPT_DIR/publish"

RIDS=("win-x64" "osx-x64" "osx-arm64" "linux-x64")
BINARY_NAME="Qubic.Net.Toolkit"

rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR"

for rid in "${RIDS[@]}"; do
    echo ""
    echo "=========================================="
    echo "  Publishing $rid"
    echo "=========================================="

    dotnet publish "$PROJECT" \
        -c Release \
        -r "$rid" \
        --self-contained \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:EnableCompressionInSingleFile=true \
        -o "$PUBLISH_DIR/$rid"

    # Determine binary filename
    if [[ "$rid" == win-* ]]; then
        bin="$BINARY_NAME.exe"
    else
        bin="$BINARY_NAME"
    fi

    zip_name="$BINARY_NAME-$rid.zip"
    bin_hash_name="$bin.sha256"
    zip_hash_name="$BINARY_NAME-$rid.zip.sha256"

    # SHA-256 hash of the binary
    sha256sum "$PUBLISH_DIR/$rid/$bin" | awk -v f="$bin" '{print $1, f}' > "$PUBLISH_DIR/$rid/$bin_hash_name"

    # Create zip containing the binary and its hash
    (cd "$PUBLISH_DIR/$rid" && zip -j "$PUBLISH_DIR/$zip_name" "$bin" "$bin_hash_name")

    # SHA-256 hash of the zip
    sha256sum "$PUBLISH_DIR/$zip_name" | awk -v f="$zip_name" '{print $1, f}' > "$PUBLISH_DIR/$zip_hash_name"

    # Clean up intermediate publish directory
    rm -rf "$PUBLISH_DIR/$rid"

    echo "  -> $zip_name (contains $bin + $bin_hash_name)"
    echo "  -> $zip_hash_name ($(awk '{print $1}' "$PUBLISH_DIR/$zip_hash_name"))"
done

echo ""
echo "=========================================="
echo "  Done! Files in: $PUBLISH_DIR"
echo "=========================================="
ls -lh "$PUBLISH_DIR"
