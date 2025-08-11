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
            // Get package info using PackageManager
            var listRequest = Client.List(
                offlineMode: true,
                includeIndirectDependencies: true);

            yield return new WaitUntil(() => listRequest.IsCompleted);

            Assert.AreEqual(StatusCode.Success, listRequest.Status,
                $"Failed to list packages: {listRequest.Error?.message}");

            // Find our package
            var package = listRequest.Result.FirstOrDefault(p => p.name == PackageName);
            Assert.IsNotNull(package, $"Package '{PackageName}' not found in project");

            // Read package.json from the package location
            var packageJsonPath = Path.Combine(package.resolvedPath, "package.json");
            Assert.IsTrue(File.Exists(packageJsonPath),
                $"package.json not found at: {packageJsonPath}");

            var packageJsonContent = File.ReadAllText(packageJsonPath);
            Assert.IsFalse(string.IsNullOrEmpty(packageJsonContent),
                "package.json content is empty");

            // Parse JSON to extract version
            var packageJson = JsonUtility.FromJson<PackageJsonData>(packageJsonContent);
            Assert.IsFalse(string.IsNullOrEmpty(packageJson.version),
                "Version not found in package.json");

            // Compare versions
            var startupVersion = Startup.Version;
            Assert.AreEqual(packageJson.version, startupVersion,
                $"Version mismatch: package.json has '{packageJson.version}' but Startup.Version is '{startupVersion}'");

            Debug.Log($"Version validation passed: {startupVersion}");
            yield return null;
        }

        [System.Serializable]
        private class PackageJsonData
        {
            public string version;
        }
    }
}