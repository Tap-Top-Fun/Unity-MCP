/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/
namespace com.IvanMurzak.Unity.MCP.Common
{
    public static partial class Consts
    {
        public static class ApiVersion
        {
            /// <summary>
            /// Current API version used for communication between Unity-MCP-Server and Unity-MCP-Plugin.
            /// When this version changes, the Plugin should display an error if connected to an incompatible Server.
            /// </summary>
            public const string Current = "1.0.0";
            
            /// <summary>
            /// Minimum supported API version for backward compatibility.
            /// </summary>
            public const string MinimumSupported = "1.0.0";
        }
    }
}