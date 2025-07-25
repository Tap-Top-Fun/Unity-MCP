#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP
{
    [Serializable]
    public class LogEntry
    {
        public string message;
        public string stackTrace;
        public LogType logType;
        public DateTime timestamp;
        public string logTypeString;

        public LogEntry(string message, string stackTrace, LogType logType)
        {
            this.message = message;
            this.stackTrace = stackTrace;
            this.logType = logType;
            this.timestamp = DateTime.Now;
            this.logTypeString = logType.ToString();
        }

        public override string ToString() => $"[{timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{logTypeString}] {message}";
    }
}
