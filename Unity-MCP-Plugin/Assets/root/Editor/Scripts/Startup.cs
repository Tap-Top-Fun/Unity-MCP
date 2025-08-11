#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    [InitializeOnLoad]
    public static partial class Startup
    {
        public const string Version = "0.14.0";

        static Startup()
        {
            McpPluginUnity.BuildAndStart();
            DownloadServerBinaryIfNeeded();

            if (Application.dataPath.Contains(" "))
                Debug.LogError("The project path contains spaces, which may cause issues during usage of Unity-MCP. Please consider the move the project to a folder without spaces.");
        }

        public static Task<bool> DownloadServerBinaryIfNeeded()
        {
            if (Server.IsBinaryExists() && Server.IsVersionMatches())
                return Task.FromResult(true);

            return DownloadServerBinary();
        }

        public static Task<bool> DownloadServerBinary()
        {
            Debug.Log($"Downloading server binary to: {Server.ExecutableFullPath}");

            // Clear existed server folder

            return Task.FromResult(false);
            // var

            // // Download the server binary
            // McpPluginUnity.DownloadServerBinaryAsync(
            //     ServerExecutableFullPath,
            //     OperationSystem,
            //     CpuArchitecture
            // );
        }
    }
}