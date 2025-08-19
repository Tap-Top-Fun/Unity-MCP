using System;
using System.Collections.Generic;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public static class ArgsUtils
    {
        public static Dictionary<string, string> ParseLineArguments(string[] args)
        {
            var providedArguments = new Dictionary<string, string>();

            // Extract flags with optional values
            for (int current = 0, next = 1; current < args.Length; current++, next++)
            {
                // Parse flag
                var isFlag = args[current].StartsWith("-");
                if (!isFlag)
                    continue;

                if (args[current].Contains("="))
                {
                    // Handle flags with '=' syntax
                    var parts = args[current].Split('=');
                    if (parts.Length == 2)
                    {
                        providedArguments[parts[0].TrimStart('-')] = parts[1];
                        continue;
                    }
                }

                var flag = args[current].TrimStart('-');

                // Parse optional value
                var flagHasValue = next < args.Length && !args[next].StartsWith("-");
                var value = flagHasValue ? args[next].TrimStart('-') : string.Empty;

                providedArguments[flag] = value;
            }

            return providedArguments;
        }

        public static Dictionary<string, string> ParseCommandLineArguments()
        {
            var args = Environment.GetCommandLineArgs();
            return ParseLineArguments(args);
        }
    }
}