#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Editor.API;
using com.IvanMurzak.Unity.MCP.Editor.Tests.Utils;
using com.IvanMurzak.Unity.MCP.Utils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public partial class AssetsMaterialTests : BaseTest
    {
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
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            yield return null;
        }

        [Test]
        public void Material_Modify()
        {
            var propertyName = "_Metallic";
            var propertyValue = 1;

            var materialEx = new CreateMaterialExecutor(
                materialName: "TestMaterial.mat",
                shaderName: "Standard",
                "Assets", "Unity-MCP-Test", "Materials"
            );

            materialEx
                .AddChild(new CallToolExecutor(
                    toolName: "Assets_Modify",
                    json: JsonTestUtils.Fill(@"{
                        ""assetPath"": ""{assetPath}"",
                        ""content"":
                        {
                            ""typeName"": ""UnityEngine.Material"",
                            ""value"": {
                                ""{propertyName}"": {propertyValue},
                            }
                        }
                    }",
                    new Dictionary<string, object?>
                    {
                        { "{assetPath}", materialEx.AssetPath },
                        { "{propertyName}", propertyName },
                        { "{propertyValue}", propertyValue }
                    }))
                )
                .AddChild(new ValidateToolResultExecutor())
                .AddChild(() =>
                {
                    var actualValue = materialEx.Asset?.GetFloat(propertyName);
                    Assert.AreEqual(propertyValue, actualValue,
                        $"Material property '{propertyName}' should be set to {propertyValue}.");
                })
                .Execute();
        }
    }
}