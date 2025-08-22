/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/
using System.Text.RegularExpressions;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public static partial class ErrorUtils
    {
        public static bool ExtractProcessId(string error, out int processId)
        {
            // Define a regex pattern to match the process ID
            var pattern = @"The file is locked by: ""[^""]+ \((\d+)\)""";
            var match = Regex.Match(error, pattern);

            processId = -1;

            return match.Success && int.TryParse(match.Groups[1].Value, out processId);
        }
    }
}
