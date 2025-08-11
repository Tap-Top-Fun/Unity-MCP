#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.IO;
using System.Runtime.InteropServices;
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
            // Sample (mac linux): ../Library/osx-x64/unity-mcp-server
            // Sample   (windows): ../Library/win-x64/unity-mcp-server.exe
            public static string ExecutableFullPath
                => Path.GetFullPath(
                    Path.Combine(
                        Application.dataPath,
                        "../Library",
                        PlatformName,
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

            // -------------------------------------------------------------------------------------------------------------------------------------------------

            public static string RawJsonConfiguration(int port, string bodyName = "mcpServers", int? timeoutMs = null)
                => Consts.MCP.Config(
                    ExecutableFullPath.Replace('\\', '/'),
                    bodyName,
                    port,
                    timeoutMs
                );

            public static string ServerExecutableUrl
                => $"https://github.com/IvanMurzak/Unity-MCP/releases/download/{Version}/{ExecutableFullName}";

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

                // Check the version of the existing binary
                var existingVersion = File.ReadAllText(ExecutableFullPath);
                return existingVersion == Version;
            }
        }
    }
}