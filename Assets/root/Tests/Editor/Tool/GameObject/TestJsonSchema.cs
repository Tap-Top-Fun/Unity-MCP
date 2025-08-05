using System;
using System.Collections;
using System.Globalization;
using System.Text.Json.Nodes;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using NUnit.Framework;
using UnityEngine.TestTools;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.Unity.MCP.Common;
using System.Text.Json;
using System.Collections.Generic;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests
{
    public class TestJsonSchema : BaseTest
    {
        static void ValidateType<T>() => ValidateType(typeof(T));
        static void ValidateType(Type type)
        {
            ValidateSchema(
                schema: JsonUtils.Schema.GetSchema(type),
                type: type);
        }
        static void ValidateSchema(JsonNode schema, Type type)
        {
            UnityEngine.Debug.Log($"Schema for '{type.GetTypeName(pretty: true)}': {schema}");

            Assert.IsNotNull(schema, $"Schema for '{type.GetTypeName(pretty: true)}' is null");

            Assert.IsFalse(schema.ToJsonString().Contains($"\"{JsonUtils.Schema.Error}\":"),
                $"Schema for '{type.GetTypeName(pretty: true)}' contains {JsonUtils.Schema.Error} string");

            var typeNodes = JsonUtils.Schema.FindAllProperties(schema, "type");
            foreach (var typeNode in typeNodes)
            {
                switch (typeNode)
                {
                    case JsonValue value:
                        var typeValue = value.ToString();
                        Assert.IsFalse(string.IsNullOrEmpty(typeValue), $"Type node for '{type.GetTypeName(pretty: true)}' is empty");
                        Assert.IsFalse(typeValue == "null", $"Type node for '{type.GetTypeName(pretty: true)}' is \"null\" string");
                        Assert.IsFalse(typeValue.Contains($"\"{JsonUtils.Schema.Error}\""), $"Type node for '{type.GetTypeName(pretty: true)}' contains error string");
                        break;
                    default:
                        if (typeNode is JsonObject typeObject)
                        {
                            if (typeObject.TryGetPropertyValue("enum", out var enumValue))
                                continue; // Skip enum types
                        }
                        Assert.Fail($"Unexpected type node for '{type.GetTypeName(pretty: true)}'.\nThe 'type' node has the type '{typeNode.GetType().GetTypeShortName()}':\n{typeNode}");
                        break;
                }
            }
        }
        static void ValidateMethodInputSchema(JsonElement schema)
        {
            ValidateMethodInputSchema(JsonNode.Parse(schema.ToString()));
        }
        static void ValidateMethodInputSchema(JsonNode schema)
        {
            UnityEngine.Debug.Log($"  Schema: {schema}");

            Assert.IsNotNull(schema, $"Schema is null");

            var json = schema.ToJsonString();

            Assert.IsNotNull(json, $"Json is null");
            Assert.IsFalse(json.Contains($"\"{JsonUtils.Schema.Error}\":"), $"Json contains {JsonUtils.Schema.Error} string");
        }

        [UnityTest]
        public IEnumerator Primitives()
        {
            ValidateType<int>();
            ValidateType<float>();
            ValidateType<bool>();
            ValidateType<string>();
            ValidateType<CultureTypes>(); // enum

            yield return null;
        }

        [UnityTest]
        public IEnumerator Classes()
        {
            // ValidateType<object>();
            ValidateType<ObjectRef>();

            ValidateType<GameObjectRef>();
            ValidateType<GameObjectRefList>();
            ValidateType<GameObjectComponentsRef>();
            ValidateType<GameObjectComponentsRefList>();

            ValidateType<ComponentData>();
            ValidateType<ComponentDataLight>();
            ValidateType<ComponentRef>();
            ValidateType<ComponentRefList>();

            ValidateType<MethodData>();
            ValidateType<MethodRef>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Structs()
        {
            ValidateType<DateTime>();
            ValidateType<TimeSpan>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator UnityStructs()
        {
            ValidateType<UnityEngine.Color32>();
            ValidateType<UnityEngine.Color>();
            ValidateType<UnityEngine.Vector3>();
            ValidateType<UnityEngine.Vector3Int>();
            ValidateType<UnityEngine.Vector2>();
            ValidateType<UnityEngine.Vector2Int>();
            ValidateType<UnityEngine.Quaternion>();
            ValidateType<UnityEngine.Matrix4x4>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Unity()
        {
            ValidateType<UnityEngine.Object>();
            ValidateType<UnityEngine.Rigidbody>();
            ValidateType<UnityEngine.Animation>();
            ValidateType<UnityEngine.Material>();
            ValidateType<UnityEngine.Transform>();
            ValidateType<UnityEngine.SpriteRenderer>();
            ValidateType<UnityEngine.MeshRenderer>();

            yield return null;
        }

        [UnityTest]
        public IEnumerator MCP_Tools()
        {
            var task = McpPlugin.Instance.McpRunner.RunListTool(new RequestListTool());
            while (!task.IsCompleted)
            {
                yield return null; // Wait for the task to complete
            }
            var toolResponse = task.Result;
            var tools = toolResponse.Value;

            Assert.IsNotNull(tools, "Tool response is null");
            Assert.IsNotEmpty(tools, "Tool response is empty");

            // Validate the array of tools doesn't have duplicated tool names
            var toolNames = new HashSet<string>();

            foreach (var tool in tools)
            {
                UnityEngine.Debug.Log($"Tool: {tool.Name} - {tool.Description}");
                ValidateMethodInputSchema(tool.InputSchema);

                Assert.IsFalse(toolNames.Contains(tool.Name), $"Duplicate tool name found: {tool.Name}");
                toolNames.Add(tool.Name);
            }
            Assert.IsTrue(toolNames.Count > 0, "No tools found in the response");
        }
    }
}