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
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public static partial class Tool_Script
    {
        [McpPluginTool
        (
            "Script_CreateOrUpdate",
            Title = "Create or Update Script"
        )]
        [Description("Creates or updates a script file with the provided content. Does AssetDatabase.Refresh() at the end.")]
        public static string UpdateOrCreate
        (
            [Description("The path to the file. Sample: \"Assets/Scripts/MyScript.cs\".")]
            string filePath,
            [Description("C# code - content of the file.")]
            string content
        )
        {
            if (string.IsNullOrEmpty(filePath))
                return Error.ScriptPathIsEmpty();

            if (!filePath.EndsWith(".cs"))
                return Error.FilePathMustEndsWithCs();

            if (!ScriptUtils.IsValidCSharpSyntax(content, out var errors))
                return $"[Error] Invalid C# syntax:\n{string.Join("\n", errors)}";

            var dirPath = Path.GetDirectoryName(filePath)!;
            if (Directory.Exists(dirPath) == false)
                Directory.CreateDirectory(dirPath);

            File.WriteAllText(filePath, content);

            return MainThread.Instance.Run(() =>
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                return $"[Success] Script created or updated at: {filePath}";
            });
        }
    }
}
