using System.IO;
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
        const string PackageId = "org.nuget";

        static ScopedRegistrySetup()
        {
            AddScopedRegistryIfNeeded(RegistryName, RegistryUrl, PackageId);
        }

        private static void AddScopedRegistryIfNeeded(string name, string url, string packageId)
        {
            var manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
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
                    ["scopes"] = new JsonArray(packageId)
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
