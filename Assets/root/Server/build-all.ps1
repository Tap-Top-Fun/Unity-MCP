#!/usr/bin/env pwsh
# Universal PowerShell script to build self-contained executables for all platforms
# Works on Windows, macOS, and Linux with PowerShell Core

param(
    [string]$Configuration = "Release",
    [string]$ProjectFile = "com.IvanMurzak.Unity.MCP.Server.csproj"
)

Write-Host "Building self-contained executables for all platforms..." -ForegroundColor Green

$runtimes = @(
    "win-x64",
    "win-x86",
    "win-arm64",
    "linux-x64",
    "linux-arm64",
    "osx-x64",
    "osx-arm64"
)

$success = 0
$failed = 0

foreach ($runtime in $runtimes) {
    Write-Host "Building for $runtime..." -ForegroundColor Yellow

    $process = Start-Process -FilePath "dotnet" -ArgumentList @(
        "publish",
        $ProjectFile,
        "-c", $Configuration,
        "-r", $runtime,
        "--self-contained", "true",
        "-p:PublishSingleFile=true"
    ) -Wait -PassThru -NoNewWindow

    if ($process.ExitCode -eq 0) {
        Write-Host "✅ Successfully built $runtime" -ForegroundColor Green
        $success++
    }
    else {
        Write-Host "❌ Failed to build $runtime" -ForegroundColor Red
        $failed++
    }
}

Write-Host "`nBuild Summary:" -ForegroundColor Cyan
Write-Host "Success: $success" -ForegroundColor Green
Write-Host "Failed: $failed" -ForegroundColor Red

if ($failed -eq 0) {
    Write-Host "`n🎉 All builds completed successfully!" -ForegroundColor Green
    Write-Host "Executables are located in: bin/$Configuration/net9.0/{runtime}/publish/" -ForegroundColor Yellow
}
else {
    Write-Host "`n⚠️  Some builds failed. Check the output above." -ForegroundColor Yellow
    exit 1
}
