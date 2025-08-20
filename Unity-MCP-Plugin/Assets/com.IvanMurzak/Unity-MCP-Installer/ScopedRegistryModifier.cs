#nullable enable
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.EditorInstaller
{
    [InitializeOnLoad]
    public static class ScopedRegistrySetup
    {
        const string RegistryName = "package.openupm.com";
        const string RegistryUrl = "https://package.openupm.com";
        static readonly string[] PackageIds = new string[] {
            "org.nuget",
            "com.IvanMurzak",
            "extensions.unity"
        };

        static string ManifestPath => Path.Combine(Application.dataPath, "../Packages/manifest.json");
        static string DependencyPackagePathRelative => "Assets/com.IvanMurzak/Unity-MCP-Installer/dependencies.unitypackage";
        static string DependencyPackagePath => Path.Combine(Application.dataPath, DependencyPackagePathRelative);

        static ScopedRegistrySetup()
        {
            AddScopedRegistryIfNeeded(RegistryName, RegistryUrl, PackageIds);
            TryImportDependenciesUnityPackage();
        }

        /// <summary>
        /// Attempts to import dependencies.unitypackage from the project root if it exists.
        /// </summary>
        public static void TryImportDependenciesUnityPackage()
        {
            // Project root is one level above Assets
            var packagePath = DependencyPackagePathRelative;
            if (File.Exists(packagePath))
            {
                AssetDatabase.ImportPackage(packagePath, false); // false = do not show import window
                Debug.Log($"Imported {packagePath}");
                AssetDatabase.DeleteAsset(packagePath);
            }
            else
            {
                Debug.Log($"dependencies.unitypackage not found at {packagePath}");
            }
        }


        [System.Serializable]
        private class ManifestFile
        {
            public ScopedRegistry[]? scopedRegistries = new ScopedRegistry[0];
        }

        [System.Serializable]
        private class ScopedRegistry
        {
            public string? name;
            public string? url;
            public string[]? scopes;
        }

        private static void AddScopedRegistryIfNeeded(string name, string url, string[] packageIds)
        {
            var manifestPath = ManifestPath;
            if (!File.Exists(manifestPath))
            {
                Debug.LogError("manifest.json not found!");
                return;
            }

            var jsonText = File.ReadAllText(manifestPath);
            ManifestFile? manifest = null;
            try
            {
                manifest = JsonUtility.FromJson<ManifestFile>(jsonText);
            }
            catch
            {
                Debug.LogError("Failed to parse manifest.json with JsonUtility");
                return;
            }
            if (manifest == null)
            {
                Debug.LogError("Failed to parse manifest.json");
                return;
            }

            if (manifest.scopedRegistries == null)
                manifest.scopedRegistries = new ScopedRegistry[0];

            bool alreadyExists = manifest.scopedRegistries.Any(r => r.name == name && r.url == url);
            if (!alreadyExists)
            {
                var newRegistry = new ScopedRegistry
                {
                    name = name,
                    url = url,
                    scopes = packageIds
                };
                var list = manifest.scopedRegistries.ToList();
                list.Add(newRegistry);
                manifest.scopedRegistries = list.ToArray();
                var newJson = JsonUtility.ToJson(manifest, true);
                File.WriteAllText(manifestPath, newJson);
                Debug.Log("Scoped registry added.");
            }
            else
            {
                Debug.Log("Scoped registry already exists.");
            }
        }
    }
}
