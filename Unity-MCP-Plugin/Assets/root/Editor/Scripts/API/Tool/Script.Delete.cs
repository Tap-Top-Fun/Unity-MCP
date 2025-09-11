/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/
#nullable enable
using System.ComponentModel;
using System.IO;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Model;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public static partial class Tool_Script
    {
        [McpPluginTool
        (
            "Script_Delete",
            Title = "Delete Script content"
        )]
        [Description("Delete the script file. Does AssetDatabase.Refresh() at the end.")]
        public static ResponseCallTool Delete
        (
            [Description("The path to the file. Sample: \"Assets/Scripts/MyScript.cs\".")]
            string filePath,
            [RequestID]
            string? requestId = null
        )
        {
            if (requestId == null || string.IsNullOrWhiteSpace(requestId))
                return ResponseCallTool.Error("Original request with valid RequestID must be provided.");

            if (string.IsNullOrEmpty(filePath))
                return ResponseCallTool.Error(Error.ScriptPathIsEmpty()).SetRequestID(requestId);

            if (!filePath.EndsWith(".cs"))
                return ResponseCallTool.Error(Error.FilePathMustEndsWithCs()).SetRequestID(requestId);

            if (File.Exists(filePath) == false)
                return ResponseCallTool.Error(Error.ScriptFileNotFound(filePath)).SetRequestID(requestId);

            File.Delete(filePath);
            if (File.Exists(filePath + ".meta"))
                File.Delete(filePath + ".meta");

            MainThread.Instance.RunAsync(() =>
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                var message = $"[Success] Script deleted: {filePath}";
                var response = ResponseCallTool.Success(message).SetRequestID(requestId);

                _ = McpPluginUnity.NotifyToolRequestCompleted(response);
            });

            return ResponseCallTool.Processing("Script deleted. Refreshing AssetDatabase...").SetRequestID(requestId);
        }
    }
}
