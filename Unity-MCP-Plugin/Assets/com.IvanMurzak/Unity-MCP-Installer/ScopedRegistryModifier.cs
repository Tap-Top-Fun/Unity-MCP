#nullable enable
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        static string DependencyPackagePath => Path.Combine(Application.dataPath, "Assets/com.IvanMurzak/Unity-MCP-Installer/dependencies.unitypackage");

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
            var packagePath = DependencyPackagePath;
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
            var root = JsonNode.Parse(jsonText)?.AsObject();
            if (root == null)
            {
                Debug.LogError("Failed to parse manifest.json");
                return;
            }

            var registries = root["scopedRegistries"] as JsonArray;
            if (registries == null)
            {
                registries = new JsonArray();
                root["scopedRegistries"] = registries;
            }

            var alreadyExists = false;
            foreach (var registryNode in registries)
            {
                var registry = registryNode as JsonObject;
                if (registry?["name"]?.ToString() == name && registry["url"]?.ToString() == url)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                var newRegistry = new JsonObject
                {
                    ["name"] = name,
                    ["url"] = url,
                    ["scopes"] = new JsonArray(packageIds.Select(id => (JsonNode)id).ToArray())
                };

                registries.Add(newRegistry);
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(manifestPath, root.ToJsonString(options));
                Debug.Log("Scoped registry added.");
            }
            else
            {
                Debug.Log("Scoped registry already exists.");
            }
        }
    }
}
