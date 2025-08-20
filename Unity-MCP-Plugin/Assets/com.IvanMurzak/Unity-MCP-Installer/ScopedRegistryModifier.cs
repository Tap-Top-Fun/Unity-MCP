#nullable enable
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.EditorInstaller
{
    [InitializeOnLoad]
    public static partial class ScopedRegistrySetup
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

            var registry = manifest.scopedRegistries.FirstOrDefault(r => r.url == url);
            if (registry == null)
            {
                // Add new registry
                var newRegistry = new ScopedRegistry
                {
                    name = name,
                    url = url,
                    scopes = packageIds.ToArray()
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
                // Add missing packageIds to existing registry
                if (registry.scopes == null)
                {
                    registry.scopes = packageIds.ToArray();
                }
                else
                {
                    var scopesSet = registry.scopes.ToList();
                    var changed = false;
                    foreach (var id in packageIds)
                    {
                        if (!scopesSet.Contains(id))
                        {
                            scopesSet.Add(id);
                            changed = true;
                        }
                    }
                    if (changed)
                    {
                        registry.scopes = scopesSet.ToArray();
                        var newJson = JsonUtility.ToJson(manifest, true);
                        File.WriteAllText(manifestPath, newJson);
                        Debug.Log("Updated existing scoped registry with new package ids.");
                    }
                    else
                    {
                        Debug.Log("Scoped registry already exists with all package ids.");
                    }
                }
            }
        }
    }
}
