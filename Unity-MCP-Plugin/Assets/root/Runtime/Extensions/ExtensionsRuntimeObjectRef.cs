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
    public static class ExtensionsRuntimeObjectRef
    {
        public static UnityEngine.Object? FindObject(this ObjectRef? objectRef)
        {
            if (objectRef == null)
                return null;

#if UNITY_EDITOR
            if (objectRef.InstanceID != 0)
                return UnityEditor.EditorUtility.InstanceIDToObject(objectRef.InstanceID);
#endif

            return null;
        }
        public static ObjectRef? ToObjectRef(this UnityEngine.Object? obj)
        {
            if (obj == null)
                return new ObjectRef();

            return new ObjectRef(obj.GetInstanceID());
        }
    }
}
