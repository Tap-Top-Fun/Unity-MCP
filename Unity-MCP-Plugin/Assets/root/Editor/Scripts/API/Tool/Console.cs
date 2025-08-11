#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.Unity.MCP.Common;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [McpPluginToolType]
    public partial class Tool_Console
    {
        public static class Error
        {
            public static string InvalidMaxEntries(int maxEntries)
                => $"[Error] Invalid maxEntries value '{maxEntries}'. Must be between 1 and {LogUtils.MaxLogEntries}.";

            public static string InvalidLogTypeFilter(string logType)
                => $"[Error] Invalid logType filter '{logType}'. Valid values: All, Error, Assert, Warning, Log, Exception.";
        }
    }
}
