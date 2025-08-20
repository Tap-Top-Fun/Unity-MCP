#nullable enable
namespace com.IvanMurzak.Unity.MCP.EditorInstaller
{
    public static partial class ScopedRegistrySetup
    {
        [System.Serializable]
        private class ManifestFile
        {
            public ScopedRegistry[]? scopedRegistries = new ScopedRegistry[0];
            public Dependencies? dependencies = new Dependencies();
        }

        [System.Serializable]
        private class Dependencies : System.Collections.Generic.Dictionary<string, string> { }

        [System.Serializable]
        private class ScopedRegistry
        {
            public string? name;
            public string? url;
            public string[]? scopes;
        }
    }
}
