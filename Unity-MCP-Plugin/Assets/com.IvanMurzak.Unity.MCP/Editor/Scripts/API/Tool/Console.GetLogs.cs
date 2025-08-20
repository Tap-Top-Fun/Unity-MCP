#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Console
    {
        [McpPluginTool
        (
            "Console_GetLogs",
            Title = "Get Unity Console Logs"
        )]
        [Description("Retrieves the Unity Console log entries. Supports filtering by log type and limiting the number of entries returned.")]
        public string GetLogs
        (
            [Description("Maximum number of log entries to return. Default: 100, Max: 5000")]
            int maxEntries = 100,
            [Description("Filter by log type. Options: 'All', 'Error', 'Assert', 'Warning', 'Log', 'Exception'. Default: 'All'")]
            string logTypeFilter = "All",
            [Description("Include stack traces in the output. Default: false")]
            bool includeStackTrace = false,
            [Description("Return logs from the last N minutes. If 0, returns all available logs. Default: 0")]
            int lastMinutes = 0
        )
        {
            return MainThread.Instance.Run(() =>
            {
                try
                {
                    // Validate parameters
                    if (maxEntries < 1 || maxEntries > LogUtils.MaxLogEntries)
                        return Error.InvalidMaxEntries(maxEntries);

                    // Parse log type filter
                    LogType? filterType = null;
                    if (logTypeFilter != "All")
                    {
                        if (!Enum.TryParse<LogType>(logTypeFilter, true, out var parsedType))
                            return Error.InvalidLogTypeFilter(logTypeFilter);
                        filterType = parsedType;
                    }

                    // Get all log entries as array to avoid concurrent modification
                    var allLogs = LogUtils.GetAllLogs().AsEnumerable();

                    // Apply time filter if specified
                    if (lastMinutes > 0)
                    {
                        var cutoffTime = DateTime.Now.AddMinutes(-lastMinutes);
                        allLogs = allLogs
                            .Where(log => log.timestamp >= cutoffTime);
                    }

                    // Apply log type filter
                    if (filterType.HasValue)
                    {
                        allLogs = allLogs
                            .Where(log => log.logType == filterType.Value);
                    }

                    // Take the most recent entries (up to maxEntries)
                    var filteredLogs = allLogs
                        .TakeLast(maxEntries)
                        .ToArray();

                    if (filteredLogs.Length == 0)
                        return "[Success] No log entries found matching the specified criteria.";

                    // Format output
                    var logLines = filteredLogs.Select(log =>
                    {
                        var line = $"[{log.timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{log.logTypeString}] {log.message}";
                        if (includeStackTrace && !string.IsNullOrEmpty(log.stackTrace))
                            line += $"\nStack Trace:\n{log.stackTrace}";

                        return line;
                    });

                    var result = string.Join("\n", logLines);
                    var summary = $"[Success] Retrieved {filteredLogs.Length} log entries";

                    if (filterType.HasValue)
                        summary += $" (filtered by {filterType.Value})";

                    if (lastMinutes > 0)
                        summary += $" (from last {lastMinutes} minutes)";

                    return $"{summary}:\n{result}";
                }
                catch (Exception ex)
                {
                    return $"[Error] Failed to retrieve console logs: {ex.Message}";
                }
            });
        }
    }
}
