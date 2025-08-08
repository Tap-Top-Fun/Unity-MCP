using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Editor.API;
using com.IvanMurzak.Unity.MCP.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class AssetsMaterialTests : BaseTest
    {
        void ValidateResult(string result)
        {
            Assert.IsFalse(string.IsNullOrEmpty(result), "Result should not be null or empty.");
            Assert.IsFalse(result.Contains("[Error]"), "Result should not contain error message.");
            Assert.IsTrue(result.Contains("[Success]"), "Result should contain success message.");
        }

        [UnityTest]
        public IEnumerator Material_Create()
        {
            var assetPath = "Assets/Materials/TestMaterial.mat";
            var result = new Tool_Assets().CreateMaterial(
                assetPath: assetPath,
                shaderName: "Standard");
            ValidateResult(result);

            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            Assert.IsNotNull(material, $"Material should be created at path: {assetPath}");
            Assert.AreEqual("Standard", material.shader.name, "Material shader should be 'Standard'.");

            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.Refresh();

            yield return null;
        }

        // [UnityTest]
        // public IEnumerator Material_Modify()
        // {
        //     var result = new Tool_Assets().Modify(
        //         assetPath: "Assets/Materials/TestMaterial.mat",
        //         shaderName: "Standard");
        //     ValidateResult(result);

        //     yield return null;
        // }
    }
}