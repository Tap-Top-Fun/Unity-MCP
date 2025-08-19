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
                    public const string PluginPort = "--plugin-port";
                    public const string PluginTimeout = "--plugin-timeout";

                    public const string ClientPort = "--client-port";
                    public const string ClientTransportMethod = "--client-transport";
                }

                public static class Env
                {
                    public const string PluginPort = "UNITY_MCP_PLUGIN_PORT";
                    public const string PluginTimeout = "UNITY_MCP_PLUGIN_TIMEOUT";

                    public const string ClientPort = "UNITY_MCP_CLIENT_PORT";
                    public const string ClientTransportMethod = "UNITY_MCP_CLIENT_TRANSPORT";
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
                                    $"{Args.PluginPort}={port}",
                                    $"{Args.PluginTimeout}={timeoutMs}",
                                    $"{Args.ClientTransportMethod}={TransportMethod.stdio}"
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