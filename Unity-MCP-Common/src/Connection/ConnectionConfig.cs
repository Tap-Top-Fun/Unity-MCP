
namespace com.IvanMurzak.Unity.MCP.Common
{
    public class ConnectionConfig
    {
        public string Endpoint { get; set; } = Consts.Hub.DefaultEndpoint;

        /// <summary>
        /// Timeout in milliseconds for MCP operations. This is set at runtime via command line args or environment variables.
        /// </summary>
        public static int TimeoutMs { get; set; } = Consts.Hub.DefaultTimeoutMs;

        public override string ToString()
            => $"Endpoint: {Endpoint}, Timeout: {TimeoutMs}ms";
    }
}