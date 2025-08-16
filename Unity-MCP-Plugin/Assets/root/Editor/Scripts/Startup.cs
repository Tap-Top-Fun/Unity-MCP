#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using UnityEditor;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    [InitializeOnLoad]
    public static partial class Startup
    {
        public const string Version = "0.14.2";

        static Startup()
        {
            McpPluginUnity.BuildAndStart(openConnection: !IsCi());
            Server.DownloadServerBinaryIfNeeded();

            if (Application.dataPath.Contains(" "))
                Debug.LogError("The project path contains spaces, which may cause issues during usage of Unity-MCP. Please consider the move the project to a folder without spaces.");
        }

        /// <summary>
        /// Checks if the current environment is a CI environment.
        /// </summary>
        public static bool IsCi()
        {
            Debug.Log($"Checking if running in CI environment...");
            Debug.Log($"CI Environment Variables: " +
                $"CI={Environment.GetEnvironmentVariable("CI")}, " +
                $"GITHUB_ACTIONS={Environment.GetEnvironmentVariable("GITHUB_ACTIONS")}, " +
                $"TF_BUILD={Environment.GetEnvironmentVariable("TF_BUILD")}");

            var ci = Environment.GetEnvironmentVariable("CI");
            var gha = Environment.GetEnvironmentVariable("GITHUB_ACTIONS");
            var az = Environment.GetEnvironmentVariable("TF_BUILD"); // Azure Pipelines
            return string.Equals(ci?.Trim()?.Trim('"'), "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(gha?.Trim()?.Trim('"'), "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(az?.Trim()?.Trim('"'), "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}