/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Model.Unity;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_GameObject
    {
        [McpPluginTool
        (
            "GameObject_Destroy",
            Title = "Destroy GameObject in opened Prefab or in a Scene"
        )]
        [Description(@"Destroy a GameObject and all nested GameObjects recursively.
Use 'instanceID' whenever possible, because it finds the exact GameObject, when 'path' may find a wrong one.")]
        public string Destroy
        (
            GameObjectRef gameObjectRef
        )
        => MainThread.Instance.Run(() =>
        {
            var go = gameObjectRef.FindGameObject(out var error);
            if (error != null)
                return $"[Error] {error}";

            Object.DestroyImmediate(go);
            return $"[Success] Destroy GameObject.";
        });
    }
}
