#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    using Consts = Common.Consts;
    public static partial class Startup
    {
        public const string ServerProjectName = "com.IvanMurzak.Unity.MCP.Server";
        public const string ServerExecutable = "unity-mcp-server";

        static string OperationSystem =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "osx" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux" :
            "unknown";

        static string CpuArchitecture => RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x64",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => "unknown"
        };

        static string ServerExecutableFullPath => Path.GetFullPath(Path.Combine(
            Application.dataPath,
            "../Library",
            $"{OperationSystem}-{CpuArchitecture}",
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? $"{ServerExecutable.ToLowerInvariant()}.exe"
                : ServerExecutable.ToLowerInvariant()));

        // Server executable path
        public static string ServerExecutableRootPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../Library", ServerProjectName.ToLowerInvariant()));
        public static string ServerExecutableFolder => Path.Combine(ServerExecutableRootPath, "bin~", "Release", "net9.0");
        public static string ServerExecutableFile => Path.Combine(ServerExecutableFolder, $"{ServerProjectName}");

        // -------------------------------------------------------------------------------------------------------------------------------------------------

        public static string RawJsonConfiguration(int port, string bodyName = "mcpServers", int? timeoutMs = null) => Consts.MCP.Config(
            ServerExecutableFile.Replace('\\', '/'),
            bodyName,
            port,
            timeoutMs
        );
    }
}