#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Collections.Concurrent;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP
{
    public static class LogUtils
    {
        public const int MaxLogEntries = 5000; // Default max entries to keep in memory

        static readonly ConcurrentQueue<LogEntry> _logEntries = new();
        static readonly object _lockObject = new();
        static bool _isSubscribed = false;

        public static int LogEntries
        {
            get
            {
                lock (_lockObject)
                {
                    return _logEntries.Count;
                }
            }
        }

        public static void ClearLogs()
        {
            lock (_lockObject)
            {
                _logEntries.Clear();
            }
        }
        public static LogEntry[] GetAllLogs()
        {
            lock (_lockObject)
            {
                return _logEntries.ToArray();
            }
        }

        static LogUtils()
        {
            EnsureSubscribed();
        }

        static void EnsureSubscribed()
        {
            MainThread.Instance.RunAsync(() =>
            {
                lock (_lockObject)
                {
                    if (!_isSubscribed)
                    {
                        Application.logMessageReceived += OnLogMessageReceived;
                        _isSubscribed = true;
                    }
                }
            });
        }

        static void OnLogMessageReceived(string message, string stackTrace, LogType type)
        {
            var logEntry = new LogEntry(message, stackTrace, type);
            lock (_lockObject)
            {
                _logEntries.Enqueue(logEntry);

                // Keep only the latest entries to prevent memory overflow
                while (_logEntries.Count > MaxLogEntries)
                    _logEntries.TryDequeue(out _);
            }
        }
    }
}
