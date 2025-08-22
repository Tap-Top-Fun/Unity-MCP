/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/

using UnityEditor;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    [InitializeOnLoad]
    static class MainWindowInitializer
    {
        const string PrefKey = "Unity-MCP.MainWindow.Initialized";

        static bool isInitialized
        {
            get => PlayerPrefs.GetInt(PrefKey, 0) == 1;
            set => PlayerPrefs.SetInt(PrefKey, value ? 1 : 0);
        }

        static MainWindowInitializer()
        {
            if (isInitialized)
                return;

            EditorApplication.delayCall += PerformInitialization;
        }

        static void PerformInitialization()
        {
            if (isInitialized)
                return;

            // Perform initialization
            McpPluginUnity.Init();
            MainWindowEditor.ShowWindow();

            isInitialized = true;
        }
    }
}