#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.ReflectorNet.Model.Unity;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsRuntimeAssetObjectRef
    {
        public static UnityEngine.Object? FindAssetObject(this AssetObjectRef? assetObjectRef)
        {
            if (assetObjectRef == null)
                return null;

#if UNITY_EDITOR
            if (assetObjectRef.instanceID != 0)
                return UnityEditor.EditorUtility.InstanceIDToObject(assetObjectRef.instanceID);

            if (!string.IsNullOrEmpty(assetObjectRef.assetPath))
                return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetObjectRef.assetPath);

            if (!string.IsNullOrEmpty(assetObjectRef.assetGuid))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(assetObjectRef.assetGuid);
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