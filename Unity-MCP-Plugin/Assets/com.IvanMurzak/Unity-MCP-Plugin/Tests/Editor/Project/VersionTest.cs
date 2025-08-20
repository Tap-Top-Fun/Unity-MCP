using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor.PackageManager;
using System.Linq;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public class StartupVersionTest
    {
        private const string PackageName = "com.ivanmurzak.unity.mcp";

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Debug.Log($"[{nameof(StartupVersionTest)}] SetUp");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Debug.Log($"[{nameof(StartupVersionTest)}] TearDown");
            yield return null;
        }

        [UnityTest]
        public IEnumerator Version_Should_Match_PackageJson_Version()
        {
            string packageJsonContent = null;
            string packageJsonPath = null;

            // Try to get package info using PackageManager first
            var listRequest = Client.List(
                offlineMode: true,
                includeIndirectDependencies: true);

            yield return new WaitUntil(() => listRequest.IsCompleted);

            if (listRequest.Status == StatusCode.Success)
            {
                // Find our package
                var package = listRequest.Result.FirstOrDefault(p => p.name == PackageName);

                if (package != null)
                {
                    // Read package.json from the package location
                    packageJsonPath = Path.Combine(package.resolvedPath, "package.json");
                    if (File.Exists(packageJsonPath))
                    {
                        packageJsonContent = File.ReadAllText(packageJsonPath);
                        Debug.Log($"Found package.json via PackageManager at: {packageJsonPath}");
                    }
                }
            }

            // Fallback: try to read from Assets/com.IvanMurzak/Unity-MCP-Plugin/package.json (for source project testing)
            if (string.IsNullOrEmpty(packageJsonContent))
            {
                var fallbackPath = Path.Combine(Application.dataPath, "com.IvanMurzak/Unity-MCP-Plugin", "package.json");
                if (File.Exists(fallbackPath))
                {
                    packageJsonContent = File.ReadAllText(fallbackPath);
                    packageJsonPath = fallbackPath;
                    Debug.Log($"Found package.json via fallback path at: {fallbackPath}");
                }
            }

            // Ensure we found a package.json file
            Assert.IsFalse(string.IsNullOrEmpty(packageJsonContent),
                $"package.json not found. Tried PackageManager location and fallback path: Assets/com.IvanMurzak/Unity-MCP-Plugin/package.json");

            // Parse JSON to extract version
            var packageJson = JsonUtility.FromJson<PackageJsonData>(packageJsonContent);
            Assert.IsFalse(string.IsNullOrEmpty(packageJson.version),
                "Version not found in package.json");

            // Compare versions
            var startupVersion = Startup.Version;
            Assert.AreEqual(packageJson.version, startupVersion,
                $"Version mismatch: package.json has '{packageJson.version}' but Startup.Version is '{startupVersion}'. Package.json path: {packageJsonPath}");

            Debug.Log($"Version validation passed: {startupVersion} (from {packageJsonPath})");
            yield return null;
        }

        [System.Serializable]
        private class PackageJsonData
        {
            public string version;
        }
    }
}