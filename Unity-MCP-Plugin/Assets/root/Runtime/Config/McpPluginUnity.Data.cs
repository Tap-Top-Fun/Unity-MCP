using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP
{
    public partial class McpPluginUnity
    {
        public class Data
        {
            public const int DefaultPort = 60606;
            public const string DefaultHost = "http://localhost:60606";

            public string Host { get; set; } = DefaultHost;
            public int Port { get; set; } = Consts.Hub.DefaultPort;
            public bool KeepConnected { get; set; } = true;
            public LogLevel LogLevel { get; set; } = LogLevel.Warning;
            public int TimeoutMs { get; set; } = Consts.Hub.DefaultTimeoutMs;

            public Data SetDefault()
            {
                Host = DefaultHost;
                Port = Consts.Hub.DefaultPort;
                KeepConnected = true;
                LogLevel = LogLevel.Warning;
                TimeoutMs = Consts.Hub.DefaultTimeoutMs;
                return this;
            }
        }
    }
}