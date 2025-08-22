/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/
using System;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class EnvironmentUtils
    {
        public static string GITHUB_ACTIONS => Environment.GetEnvironmentVariable("GITHUB_ACTIONS");
        public static bool IsGitHubActions => GITHUB_ACTIONS == "true";
    }
}
