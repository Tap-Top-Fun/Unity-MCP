using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public class BaseTest
    {
        [UnitySetUp]
        public virtual IEnumerator SetUp()
        {
            Debug.Log($"[{GetType().Name}] SetUp");

            McpPluginUnity.Init();

            yield return null;
        }
        [UnityTearDown]
        public virtual IEnumerator TearDown()
        {
            Debug.Log($"[{GetType().Name}] TearDown");

            DestroyAllGameObjectsInActiveScene();

            yield return null;
        }

        protected static void DestroyAllGameObjectsInActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
                Object.DestroyImmediate(go);
        }

        protected virtual IResponseData<ResponseCallTool> RunTool(string toolName, string json)
        {
            Debug.Log($"{toolName} Started with JSON:\n{json}");
            var parameters = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            var request = new RequestCallTool(toolName, parameters);
            var task = McpPlugin.Instance.McpRunner.RunCallTool(request);
            var result = task.Result;

            Debug.Log($"{toolName} Completed");

            var jsonResult = JsonUtils.ToJson(result);
            Debug.Log($"{toolName} Result:\n{jsonResult}");

            Assert.IsFalse(result.IsError);
            Assert.IsFalse(result.Message.Contains("[Error]"), $"Tool call failed with error: {result.Message}");
            Assert.IsFalse(result.Value.IsError, $"Tool call failed");
            Assert.IsFalse(jsonResult.Contains("[Error]"), $"Tool call failed with error in JSON: {jsonResult}");
            Assert.IsFalse(jsonResult.Contains("[Warning]"), $"Tool call contains warnings in JSON: {jsonResult}");

            return result;
        }
    }
}