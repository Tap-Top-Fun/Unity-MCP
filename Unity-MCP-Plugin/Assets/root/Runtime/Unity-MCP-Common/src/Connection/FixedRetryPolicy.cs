
using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public class FixedRetryPolicy : IRetryPolicy
    {
        private readonly TimeSpan _delay;

        public FixedRetryPolicy(TimeSpan delay)
        {
            _delay = delay;
        }

        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            return _delay;
        }
    }
}