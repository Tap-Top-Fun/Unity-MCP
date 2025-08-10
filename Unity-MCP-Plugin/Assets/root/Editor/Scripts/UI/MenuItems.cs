#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#if UNITY_EDITOR
using UnityEditor;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    public static class MenuItems
    {
        [MenuItem("Window/AI Connector (Unity-MCP)", priority = 1006)]
        public static void ShowWindow() => MainWindowEditor.ShowWindow();
    }
}
#endif