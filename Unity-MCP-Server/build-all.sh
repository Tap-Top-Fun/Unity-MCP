#!/bin/bash
# Universal bash script to build self-contained executables for all platforms
# Works on macOS and Linux

set -euo pipefail

CONFIGURATION="${1:-Release}"
PROJECT_FILE="${2:-com.IvanMurzak.Unity.MCP.Server.csproj}"

SCRIPT_DIR="$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
PUBLISH_ROOT="${SCRIPT_DIR}/publish"

echo "🚀 Building self-contained executables for all platforms..."

# Clean publish root
if [ -d "${PUBLISH_ROOT}" ]; then
    echo "🧹 Cleaning existing publish folder..."
    rm -rf "${PUBLISH_ROOT}" || { echo "Failed to remove publish folder"; exit 1; }
fi
mkdir -p "${PUBLISH_ROOT}"

runtimes=(
    "win-x64"
    "win-x86"
    "win-arm64"
    "linux-x64"
    "linux-arm64"
    "osx-x64"
    "osx-arm64"
)

success=0
failed=0

for runtime in "${runtimes[@]}"; do
    echo "🔨 Building for ${runtime}..."

    OUTPUT_PATH="${PUBLISH_ROOT}/${runtime}"
    mkdir -p "${OUTPUT_PATH}"

    if dotnet publish "${PROJECT_FILE}" \
        -c "${CONFIGURATION}" \
        -r "${runtime}" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -o "${OUTPUT_PATH}"; then
        echo "✅ Successfully built ${runtime} -> ${OUTPUT_PATH}"
        ((success++))
    else
        echo "❌ Failed to build ${runtime}"
        ((failed++))
    fi
    echo ""
done

echo "📊 Build Summary:"
echo "Success: $success"
echo "Failed: $failed"

if [ $failed -eq 0 ]; then
    echo ""
    echo "🎉 All builds completed successfully!"
    echo "📁 Executables are located in: ${PUBLISH_ROOT}/{runtime}/"
else
    echo ""
    echo "⚠️  Some builds failed. Check the output above. Partial outputs: ${PUBLISH_ROOT}" 
    exit 1
fi
