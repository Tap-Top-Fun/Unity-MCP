using System.Text.Json.Nodes;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public static partial class Consts
    {
        // public const string All = "*";
        // public const string AllRecursive = "**";
        // public const string PackageName = "com.ivanmurzak.unity.mcp";

        public static class Guid
        {
            public const string Zero = "00000000-0000-0000-0000-000000000000";
        }

        public static partial class Command
        {
            public static partial class ResponseCode
            {
                public const string Success = "[Success]";
                public const string Error = "[Error]";
                public const string Cancel = "[Cancel]";
            }
        }
        public static class MCP
        {
            public const int LinesLimit = 1000;

            public static JsonNode Config(string executablePath, string serverName, string bodyName, int port, int? timeoutMs = null)
                => new JsonObject
                {
                    [bodyName] = new JsonObject
                    {
                        [serverName] = new JsonObject
                        {
                            ["command"] = executablePath,
                            ["args"] = new JsonArray
                            {
                                $"--port={port}",
                                $"--timeout={timeoutMs ?? Hub.DefaultTimeoutMs}"
                            }
                        }
                    }
                };
        }
    }
}