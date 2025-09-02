/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.Unity.MCP.Common.Model;
using Microsoft.Extensions.Logging;
using R3;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public interface IRequestTrackingService
    {
        Task<IResponseData<TResponse>> TrackRequestAsync<TResponse>(string requestId, Func<Task<IResponseData<TResponse>>> executeRequest, int timeoutMs, CancellationToken cancellationToken = default);
        void CompleteRequest<TResponse>(string requestId, IResponseData<TResponse> response);
        void NotifyDomainReload(string connectionId);
        void RegisterPendingRequests(string connectionId, string[] requestIds);
    }

    public class RequestTrackingService : IRequestTrackingService, IDisposable
    {
        readonly ILogger<RequestTrackingService> _logger;
        readonly ConcurrentDictionary<string, PendingRequest> _pendingRequests = new();
        readonly CompositeDisposable _disposables = new();

        public RequestTrackingService(ILogger<RequestTrackingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogTrace("RequestTrackingService initialized");
        }

        static bool IsSignalRConnectionException(Exception ex)
        {
            return ex switch
            {
                Microsoft.AspNetCore.SignalR.HubException => true,
                InvalidOperationException invalidOp when invalidOp.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,
                OperationCanceledException => true,
                System.Net.Sockets.SocketException => true,
                System.IO.IOException => true,
                _ when ex.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,
                _ when ex.Message.Contains("SignalR", StringComparison.OrdinalIgnoreCase) => true,
                _ when ex.InnerException != null => IsSignalRConnectionException(ex.InnerException),
                _ => false
            };
        }

        public async Task<IResponseData<TResponse>> TrackRequestAsync<TResponse>(
            string requestId,
            Func<Task<IResponseData<TResponse>>> executeRequest,
            int timeoutMs,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(requestId))
                throw new ArgumentException("RequestId cannot be null or empty", nameof(requestId));

            _logger.LogTrace("Tracking request: {RequestId} with timeout: {TimeoutMs}ms", requestId, timeoutMs);

            var pendingRequest = new PendingRequest<TResponse>(requestId, timeoutMs);
            _pendingRequests[requestId] = pendingRequest;

            try
            {
                var initialResponse = await executeRequest();

                if (initialResponse.Status != ResponseStatus.Processing)
                {
                    _logger.LogTrace("Request {RequestId} completed immediately", requestId);
                    _pendingRequests.TryRemove(requestId, out _);
                    return initialResponse;
                }

                if (initialResponse.Message?.Contains("domain reload", StringComparison.OrdinalIgnoreCase) == true)
                {
                    _logger.LogInformation("Request {RequestId} requires domain reload, waiting for completion", requestId);
                    return await pendingRequest.WaitForCompletion<TResponse>(cancellationToken);
                }

                _logger.LogTrace("Request {RequestId} failed without domain reload: {Message}", requestId, initialResponse.Message);
                _pendingRequests.TryRemove(requestId, out _);
                return initialResponse;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogInformation(ex, "SignalR connection lost for request {RequestId}, likely due to domain reload. Waiting for completion via dedicated SignalR call", requestId);
                return await pendingRequest.WaitForCompletion<TResponse>(cancellationToken);
            }
            catch (Exception ex) when (IsSignalRConnectionException(ex))
            {
                _logger.LogInformation(ex, "SignalR connection lost for request {RequestId}, likely due to domain reload. Waiting for completion via dedicated SignalR call", requestId);
                return await pendingRequest.WaitForCompletion<TResponse>(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing request {RequestId}", requestId);
                _pendingRequests.TryRemove(requestId, out _);
                throw;
            }
        }

        public void CompleteRequest<TResponse>(string requestId, IResponseData<TResponse> response)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                _logger.LogWarning("Attempted to complete request with null or empty RequestId");
                return;
            }

            if (_pendingRequests.TryRemove(requestId, out var pendingRequest))
            {
                _logger.LogTrace("Completing tracked request: {RequestId}", requestId);
                if (pendingRequest is PendingRequest<TResponse> typedPendingRequest)
                {
                    typedPendingRequest.Complete(response);
                }
                else
                {
                    _logger.LogWarning("Type mismatch when completing request {RequestId}. Expected {ExpectedType}, got {ActualType}",
                        requestId, typeof(TResponse).Name, pendingRequest.GetType().Name);
                }
            }
            else
            {
                _logger.LogWarning("Attempted to complete request {RequestId} that is not being tracked", requestId);
            }
        }

        public void NotifyDomainReload(string connectionId)
        {
            _logger.LogInformation("Domain reload notification received from connection: {ConnectionId}", connectionId);

            var activeRequests = 0;
            foreach (var kvp in _pendingRequests)
            {
                if (kvp.Value.ConnectionId == connectionId && !kvp.Value.IsCompleted)
                {
                    activeRequests++;
                }
            }

            _logger.LogInformation("Connection {ConnectionId} has {ActiveRequests} active requests after domain reload",
                connectionId, activeRequests);
        }

        public void RegisterPendingRequests(string connectionId, string[] requestIds)
        {
            if (requestIds == null || requestIds.Length == 0)
            {
                _logger.LogTrace("No pending requests to register for connection: {ConnectionId}", connectionId);
                return;
            }

            _logger.LogInformation("Registering {Count} pending requests for connection: {ConnectionId}",
                requestIds.Length, connectionId);

            foreach (var requestId in requestIds)
            {
                if (_pendingRequests.TryGetValue(requestId, out var pendingRequest))
                {
                    pendingRequest.ConnectionId = connectionId;
                    _logger.LogTrace("Updated connection for pending request: {RequestId}", requestId);
                }
                else
                {
                    _logger.LogWarning("Attempted to register unknown pending request: {RequestId}", requestId);
                }
            }
        }

        public void Dispose()
        {
            _logger.LogTrace("RequestTrackingService disposing");

            foreach (var kvp in _pendingRequests.ToArray())
            {
                try
                {
                    kvp.Value.Cancel();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error canceling pending request {RequestId} during disposal", kvp.Key);
                }
            }

            _pendingRequests.Clear();
            _disposables.Dispose();
        }

        abstract class PendingRequest
        {
            public string RequestId { get; }
            public string? ConnectionId { get; set; }
            public bool IsCompleted { get; protected set; }
            protected readonly CancellationTokenSource TimeoutCts;

            protected PendingRequest(string requestId, int timeoutMs)
            {
                RequestId = requestId;
                TimeoutCts = new CancellationTokenSource(timeoutMs);
                IsCompleted = false;
            }

            public abstract Task<IResponseData<T>> WaitForCompletion<T>(CancellationToken cancellationToken = default);
            public abstract void Cancel();
        }

        class PendingRequest<T> : PendingRequest
        {
            readonly TaskCompletionSource<IResponseData<T>> _completionSource = new();

            public PendingRequest(string requestId, int timeoutMs) : base(requestId, timeoutMs)
            {
                TimeoutCts.Token.Register(() =>
                {
                    if (!IsCompleted)
                    {
                        IsCompleted = true;
                        _completionSource.TrySetResult(
                            ResponseData<T>.Error(RequestId, $"Request {RequestId} timed out after {timeoutMs}ms"));
                    }
                });
            }

            public void Complete(IResponseData<T> response)
            {
                if (!IsCompleted)
                {
                    IsCompleted = true;
                    _completionSource.TrySetResult(response);
                    TimeoutCts.Cancel();
                }
            }

            public override async Task<IResponseData<TResult>> WaitForCompletion<TResult>(CancellationToken cancellationToken = default)
            {
                if (typeof(TResult) != typeof(T))
                {
                    return ResponseData<TResult>.Error(RequestId,
                        $"Type mismatch: expected {typeof(TResult).Name}, got {typeof(T).Name}");
                }

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(TimeoutCts.Token, cancellationToken);

                try
                {
                    var response = await _completionSource.Task.WaitAsync(linkedCts.Token);
                    return (IResponseData<TResult>)(object)response;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return ResponseData<TResult>.Error(RequestId, "Request was canceled");
                }
                catch (OperationCanceledException) when (TimeoutCts.Token.IsCancellationRequested)
                {
                    return ResponseData<TResult>.Error(RequestId, "Request timed out");
                }
            }

            public override void Cancel()
            {
                if (!IsCompleted)
                {
                    IsCompleted = true;
                    _completionSource.TrySetCanceled();
                    TimeoutCts.Cancel();
                }
            }
        }
    }
}