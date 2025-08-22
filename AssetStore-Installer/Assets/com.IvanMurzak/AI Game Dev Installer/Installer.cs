#nullable enable
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Installer
{
    [InitializeOnLoad]
    public static partial class Installer
    {
        public const string PackageId = "com.ivanmurzak.unity.mcp";
        public const string Version = "0.15.0";

        static Installer()
        {
            AddScopedRegistryIfNeeded(ManifestPath);
        }
    }
}