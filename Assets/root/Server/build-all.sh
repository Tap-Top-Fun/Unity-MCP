#!/bin/bash
# Universal bash script to build self-contained executables for all platforms
# Works on macOS and Linux

CONFIGURATION="${1:-Release}"
PROJECT_FILE="${2:-com.IvanMurzak.Unity.MCP.Server.csproj}"

echo "🚀 Building self-contained executables for all platforms..."

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
    echo "🔨 Building for $runtime..."

    if dotnet publish "$PROJECT_FILE" \
        -c "$CONFIGURATION" \
        -r "$runtime" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=true \
        -p:TrimMode=partial; then
        echo "✅ Successfully built $runtime"
        ((success++))
    else
        echo "❌ Failed to build $runtime"
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
    echo "📁 Executables are located in: bin/$CONFIGURATION/net9.0/{runtime}/publish/"
else
    echo ""
    echo "⚠️  Some builds failed. Check the output above."
    exit 1
fi
