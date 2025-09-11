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

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsRuntimeAssetObjectRef
    {
        public static UnityEngine.Object? FindAssetObject(this AssetObjectRef? assetObjectRef)
        {
            if (assetObjectRef == null)
                return null;

#if UNITY_EDITOR
            if (assetObjectRef.InstanceID != 0)
                return UnityEditor.EditorUtility.InstanceIDToObject(assetObjectRef.InstanceID);

            if (!string.IsNullOrEmpty(assetObjectRef.AssetPath))
                return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetObjectRef.AssetPath);

            if (!string.IsNullOrEmpty(assetObjectRef.AssetGuid))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(assetObjectRef.AssetGuid);
                if (!string.IsNullOrEmpty(path))
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            }
#endif

            return null;
        }
        public static AssetObjectRef? ToAssetObjectRef(this UnityEngine.Object? obj)
        {
            if (obj == null)
                return new AssetObjectRef();

            return new AssetObjectRef(obj.GetInstanceID());
        }
    }
}
