#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.Unity.MCP.Common;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.Tests.Utils
{
    public class CallToolExecutor : LazyNodeExecutor
    {
        public CallToolExecutor(string toolName, string json, Reflector? reflector = null) : base()
        {
            if (toolName == null) throw new ArgumentNullException(nameof(toolName));
            if (json == null) throw new ArgumentNullException(nameof(json));

            reflector ??= McpPlugin.Instance!.McpRunner.Reflector ??
                throw new ArgumentNullException(nameof(reflector), "Reflector cannot be null. Ensure McpPlugin is initialized before using this executor.");

            SetAction(() =>
            {
                Debug.Log($"{toolName} Started with JSON:\n{JsonTestUtils.Prettify(json)}");

                var parameters = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                var request = new RequestCallTool(toolName, parameters!);
                var task = McpPlugin.Instance!.McpRunner.RunCallTool(request);
                var result = task.Result;

                Debug.Log($"{toolName} Completed");

                return result;
            });
        }
    }
}