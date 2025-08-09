#if !UNITY_5_3_OR_NEWER
using System;
using System.Threading;
using System.Threading.Tasks;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public static class ExtensionsTask
    {
        const int DefaultTimeoutMs = 60000; // Default timeout in milliseconds

        public static Task<bool> WaitWithTimeout(this Task task, int timeoutMs = DefaultTimeoutMs, CancellationToken cancellationToken = default)
            => WaitWithTimeout(task, TimeSpan.FromMilliseconds(timeoutMs), cancellationToken);

        public static async Task<bool> WaitWithTimeout(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            try
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cancellationToken));
                return completedTask == task;
            }
            catch (OperationCanceledException)
            {
                return false; // Task was cancelled
            }
        }

        public static Task<bool> WaitWithTimeout<T>(this Task<T> task, int timeoutMs = DefaultTimeoutMs, CancellationToken cancellationToken = default)
            => WaitWithTimeout(task, TimeSpan.FromMilliseconds(timeoutMs), cancellationToken);

        public static async Task<bool> WaitWithTimeout<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            try
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cancellationToken));
                return completedTask == task;
            }
            catch (OperationCanceledException)
            {
                return false; // Task was cancelled
            }
        }
    }
}
#endif