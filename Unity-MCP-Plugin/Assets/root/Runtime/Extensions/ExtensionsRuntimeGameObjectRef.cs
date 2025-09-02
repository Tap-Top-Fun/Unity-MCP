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
using com.IvanMurzak.Unity.MCP.Common.Model.Unity;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsRuntimeGameObjectRef
    {
        public static GameObject? FindGameObject(this GameObjectRef? objectRef)
            => FindGameObject(objectRef, out _);
        public static GameObject? FindGameObject(this GameObjectRef? objectRef, out string? error)
        {
            if (objectRef == null)
            {
                error = null;
                return null;
            }

            var go = GameObjectUtils.FindBy(objectRef, out error);
            if (go == null)
                go = ExtensionsRuntimeAssetObjectRef.FindAssetObject(objectRef) as GameObject;

            if (go != null)
                error = null;

            return go;
        }
        public static GameObjectRef? ToGameObjectRef(this GameObject? obj)
        {
            if (obj == null)
                return new GameObjectRef();

            return new GameObjectRef(obj.GetInstanceID());
        }
    }
}
