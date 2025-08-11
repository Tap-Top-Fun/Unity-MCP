#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    using Consts = Common.Consts;

    public static partial class Startup
    {
        public static class Server
        {
            public const string ExecutableName = "unity-mcp-server";

            public static string OperationSystem =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
                "unknown";

            public static string CpuArch => RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => "x86",
                Architecture.X64 => "x64",
                Architecture.Arm => "arm",
                Architecture.Arm64 => "arm64",
                _ => "unknown"
            };

            public static string PlatformName => $"{OperationSystem}-{CpuArch}";

            // Server executable file name
            // Sample (mac linux): unity-mcp-server
            // Sample   (windows): unity-mcp-server.exe
            public static string ExecutableFullName
                => ExecutableName.ToLowerInvariant() + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? ".exe"
                    : string.Empty);

            // Full path to the server executable
            // Sample (mac linux): ../Library/mcp-server/osx-x64
            // Sample   (windows): ../Library/mcp-server/win-x64
            public static string ExecutableFolderPath
                => Path.GetFullPath(
                    Path.Combine(
                        Application.dataPath,
                        "../Library",
                        "mcp-server",
                        PlatformName
                    )
                );

            // Full path to the server executable
            // Sample (mac linux): ../Library/mcp-server/osx-x64/unity-mcp-server
            // Sample   (windows): ../Library/mcp-server/win-x64/unity-mcp-server.exe
            public static string ExecutableFullPath
                => Path.GetFullPath(
                    Path.Combine(
                        ExecutableFolderPath,
                        ExecutableFullName
                    )
                );

            public static string VersionFullPath
                => Path.GetFullPath(
                    Path.Combine(
                        Application.dataPath,
                        "../Library",
                        PlatformName,
                        "version"
                    )
                );

            // ------------------------------------------------------------------------------------------------------------------------------------

            public static string RawJsonConfiguration(int port, string bodyName = "mcpServers", int? timeoutMs = null)
                => Consts.MCP.Config(
                    ExecutableFullPath.Replace('\\', '/'),
                    bodyName,
                    port,
                    timeoutMs
                );

            public static string ExecutableZipUrl
                => $"https://github.com/IvanMurzak/Unity-MCP/releases/download/{Version}/{ExecutableName.ToLowerInvariant()}-{PlatformName}.zip";

            // ------------------------------------------------------------------------------------------------------------------------------------

            public static bool IsBinaryExists()
            {
                if (string.IsNullOrEmpty(ExecutableFullPath))
                    return false;

                return File.Exists(ExecutableFullPath);
            }

            public static bool IsVersionMatches()
            {
                if (string.IsNullOrEmpty(ExecutableFullPath))
                    return false;

                if (!File.Exists(ExecutableFullPath))
                    return false;

                // Check the version of the existing binary
                var existingVersion = File.ReadAllText(ExecutableFullPath);
                return existingVersion == Version;
            }

            public static Task<bool> DownloadServerBinaryIfNeeded()
            {
                if (IsBinaryExists() && IsVersionMatches())
                    return Task.FromResult(true);

                return DownloadAndUnpackBinary();
            }

            public static async Task<bool> DownloadAndUnpackBinary()
            {
                Debug.Log($"Downloading Unity-MCP-Server binary to: {ExecutableFolderPath}");

                try
                {
                    // Clear existed server folder
                    if (Directory.Exists(ExecutableFolderPath))
                        Directory.Delete(ExecutableFolderPath, true);

                    // Create folder if needed
                    if (!Directory.Exists(ExecutableFolderPath))
                        Directory.CreateDirectory(ExecutableFolderPath);

                    var archiveFilePath = $"{Application.temporaryCachePath}/{ExecutableName.ToLowerInvariant()}-{PlatformName}-{Version}.zip";

                    // Download the zip file from the GitHub release notes
                    using (var client = new WebClient())
                    {
                        await client.DownloadFileTaskAsync(ExecutableZipUrl, archiveFilePath);
                    }

                    // Unpack zip archive
                    ZipFile.ExtractToDirectory(archiveFilePath, ExecutableFolderPath);

                    if (!File.Exists(ExecutableFullPath))
                    {
                        Debug.LogError($"Failed to unpack server binary to: {ExecutableFullPath}");
                        return false;
                    }

                    File.WriteAllText(VersionFullPath, Version);

                    return IsBinaryExists() && IsVersionMatches();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to download and unpack server binary: {ex.Message}");
                    Debug.LogException(ex);
                    return false;
                }
            }
        }
    }
}