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
using com.IvanMurzak.Unity.MCP.Common.Model;
using Microsoft.Extensions.Logging;
using R3;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public interface IRequestTrackingService
    {
        Task<IResponseData<TResponse>> TrackRequestAsync<TResponse>(
            string requestId,
            Func<Task<IResponseData<TResponse>>> executeRequest,
            int timeoutMs,
            CancellationToken cancellationToken = default);
        void CompleteRequest(IResponseData response);
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

        public async Task<IResponseData<TResponse>> TrackRequestAsync<TResponse>(
            string requestId,
            Func<Task<IResponseData<TResponse>>> executeRequest,
            int timeoutMs,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(requestId))
                throw new ArgumentException("RequestId cannot be null or empty", nameof(requestId));

            _logger.LogTrace("Tracking request: {RequestId} with timeout: {TimeoutMs}ms", requestId, timeoutMs);

            var pendingRequest = new PendingRequest(requestId, timeoutMs);
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

                _logger.LogTrace("Request {RequestId} failed without domain reload: {Message}", requestId, initialResponse.Message);
                _pendingRequests.TryRemove(requestId, out _);
                return initialResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing request {RequestId}", requestId);
                _pendingRequests.TryRemove(requestId, out _);
                throw;
            }
        }

        public void CompleteRequest(IResponseData response)
        {
            if (string.IsNullOrEmpty(response.RequestID))
            {
                _logger.LogWarning("Attempted to complete request with null or empty RequestId");
                return;
            }

            if (_pendingRequests.TryRemove(response.RequestID, out var pendingRequest))
            {
                _logger.LogTrace("Completing tracked request: {RequestId}", response.RequestID);
                if (pendingRequest is PendingRequest typedPendingRequest)
                {
                    typedPendingRequest.Complete(response);
                }
                else
                {
                    _logger.LogWarning("Type mismatch when completing request {RequestId}. Expected {ExpectedType}, got {ActualType}",
                        response.RequestID, nameof(IResponseData), pendingRequest.GetType().Name);
                }
            }
            else
            {
                _logger.LogWarning("Attempted to complete request {RequestId} that is not being tracked", response.RequestID);
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

        class PendingRequest
        {
            public string RequestId { get; }
            public string? ConnectionId { get; set; }
            public bool IsCompleted { get; protected set; }
            protected readonly CancellationTokenSource TimeoutCts;

            readonly TaskCompletionSource<IResponseData> _completionSource = new();

            public PendingRequest(string requestId, int timeoutMs)
            {

                RequestId = requestId;
                TimeoutCts = new CancellationTokenSource(timeoutMs);
                IsCompleted = false;

                TimeoutCts.Token.Register(() =>
                {
                    if (!IsCompleted)
                    {
                        IsCompleted = true;
                        _completionSource.TrySetResult(
                            ResponseData.Error(RequestId, $"Request {RequestId} timed out after {timeoutMs}ms"));
                    }
                });
            }

            public void Complete(IResponseData response)
            {
                if (!IsCompleted)
                {
                    IsCompleted = true;
                    _completionSource.TrySetResult(response);
                    TimeoutCts.Cancel();
                }
            }

            public async Task<IResponseCallTool> WaitForCompletion(CancellationToken cancellationToken = default)
            {

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(TimeoutCts.Token, cancellationToken);

                try
                {
                    var response = await _completionSource.Task.WaitAsync(linkedCts.Token);
                    return (IResponseCallTool)(object)response;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return ResponseCallTool.Error("Request was canceled").SetRequestID(RequestId);
                }
                catch (OperationCanceledException) when (TimeoutCts.Token.IsCancellationRequested)
                {
                    return ResponseCallTool.Error("Request timed out").SetRequestID(RequestId);
                }
            }

            public void Cancel()
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