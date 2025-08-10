#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.IO;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    using Consts = Common.Consts;
    public static partial class Startup
    {
        public const string ServerProjectName = "com.IvanMurzak.Unity.MCP.Server";

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