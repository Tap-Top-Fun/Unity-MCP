#nullable enable
using System.IO;
using System.Linq;
using UnityEngine;
using SimpleJSON;

namespace com.IvanMurzak.Unity.MCP.EditorInstaller
{
    public static partial class Installer
    {
        static string ManifestPath => Path.Combine(Application.dataPath, "../Packages/manifest.json");

        // Property names
        const string Dependencies = "dependencies";
        const string ScopedRegistries = "scopedRegistries";
        const string Name = "name";
        const string Url = "url";
        const string Scopes = "scopes";

        // Property values
        const string RegistryName = "package.openupm.com";
        const string RegistryUrl = "https://package.openupm.com";
        static readonly string[] PackageIds = new string[] {
            "org.nuget",
            "com.ivanmurzak",
            "extensions.unity"
        };

        static void AddScopedRegistryIfNeeded()
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogError($"{ManifestPath} not found!");
                return;
            }
            var jsonText = File.ReadAllText(ManifestPath);

            var manifestJson = JSONObject.Parse(jsonText);
            if (manifestJson == null)
            {
                Debug.LogError($"Failed to parse {ManifestPath} as JSON.");
                return;
            }

            var modified = false;

            // --- Add scoped registries if needed
            var scopedRegistries = manifestJson[ScopedRegistries];
            if (scopedRegistries == null)
            {
                manifestJson[ScopedRegistries] = new JSONArray();
                modified = true;
            }

            // --- Add OpenUPM registry if needed
            var openUpmRegistry = scopedRegistries!.Linq
                .Select(kvp => kvp.Value)
                .Where(r => r.Linq
                    .Any(p => p.Key == Name && p.Value == RegistryName))
                .FirstOrDefault();

            if (openUpmRegistry == null)
            {
                scopedRegistries.Add(openUpmRegistry = new JSONObject
                {
                    [Name] = RegistryName,
                    [Url] = RegistryUrl,
                    [Scopes] = new JSONArray()
                });
                modified = true;
            }

            // --- Add missing scopes
            var scopes = openUpmRegistry[Scopes];
            if (scopes == null)
            {
                openUpmRegistry[Scopes] = new JSONArray();
                modified = true;
            }
            foreach (var packageId in PackageIds)
            {
                var existingScope = scopes!.Linq
                    .Select(kvp => kvp.Value)
                    .Where(value => value == packageId)
                    .FirstOrDefault();
                if (existingScope == null)
                {
                    scopes.Add(packageId);
                    modified = true;
                }
            }

            // --- Package Dependency
            var dependencies = manifestJson[Dependencies];
            if (dependencies == null)
            {
                manifestJson[Dependencies] = dependencies = new JSONObject();
                modified = true;
            }
            if (dependencies[PackageId] != Version)
            {
                dependencies[PackageId] = Version;
                modified = true;
            }

            // --- Write changes back to manifest
            if (modified)
                File.WriteAllText(ManifestPath, manifestJson.ToString(2));
        }
    }
}