#nullable enable

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsRuntimeObject
    {
        public static bool IsAsset(this UnityEngine.Object? obj)
        {
            if (obj == null)
                return false;

#if UNITY_EDITOR
            if (obj == null) return false;
            if (!UnityEditor.EditorUtility.IsPersistent(obj))
                return false; // not stored on disk

            var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
            return !string.IsNullOrEmpty(path) && (path.StartsWith("Assets/") || path.StartsWith("Packages/"));
#else
            return false;
#endif
        }
    }
}