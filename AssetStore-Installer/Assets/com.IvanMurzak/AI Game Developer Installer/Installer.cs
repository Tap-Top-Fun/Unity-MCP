#nullable enable
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.EditorInstaller
{
    [InitializeOnLoad]
    public static partial class Installer
    {
        const string PackageId = "com.ivanmurzak.unity.mcp";
        const string Version = "0.15.0";

        static Installer()
        {
            AddScopedRegistryIfNeeded();
        }
    }
}