#nullable enable
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