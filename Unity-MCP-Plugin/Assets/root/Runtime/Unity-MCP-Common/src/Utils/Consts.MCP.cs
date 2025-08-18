using System.Text.Json.Nodes;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public static partial class Consts
    {
        public static partial class MCP
        {
            public static class Plugin
            {
                public const int LinesLimit = 1000;
            }
            public static class Server
            {
                public static partial class Args
                {
                    public const string Port = "--unity-port";
                    public const string Timeout = "--unity-timeout";
                    public const string TransportMethod = "--transport";
                }

                public static class Env
                {
                    public const string Port = "UNITY_MCP_PORT";
                    public const string Timeout = "UNITY_MCP_TIMEOUT";
                    public const string TransportMethod = "UNITY_MCP_TRANSPORT";
                }

                public static JsonNode Config(
                    string executablePath,
                    string serverName = "Unity-MCP",
                    string bodyName = "mcpServers",
                    int port = Hub.DefaultPort,
                    int timeoutMs = Hub.DefaultTimeoutMs)
                {
                    return new JsonObject
                    {
                        [bodyName] = new JsonObject
                        {
                            [serverName] = new JsonObject
                            {
                                ["command"] = executablePath,
                                ["args"] = new JsonArray
                                {
                                    $"{Args.Port}={port}",
                                    $"{Args.Timeout}={timeoutMs}",
                                    $"{Args.TransportMethod}={TransportMethod.stdio}"
                                }
                            }
                        }
                    };
                }

                public enum TransportMethod
                {
                    unknown,
                    stdio,
                    http
                }
            }
        }
    }
}