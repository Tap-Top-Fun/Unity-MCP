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
using System.Text.Json;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Common.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public class RemoteApp : BaseHub<RemoteApp>, IRemoteApp
    {
        readonly EventAppToolsChange _eventAppToolsChange;
        readonly IRequestTrackingService _requestTrackingService;

        public RemoteApp(ILogger<RemoteApp> logger, IHubContext<RemoteApp> hubContext, EventAppToolsChange eventAppToolsChange, IRequestTrackingService requestTrackingService)
            : base(logger, hubContext)
        {
            _eventAppToolsChange = eventAppToolsChange ?? throw new ArgumentNullException(nameof(eventAppToolsChange));
            _requestTrackingService = requestTrackingService ?? throw new ArgumentNullException(nameof(requestTrackingService));
        }

        static JsonSerializerOptions CreateJsonOptions() => new()
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public Task<IResponseData<string>> OnListToolsUpdated(string data)
        {
            _logger.LogTrace("RemoteApp OnListToolsUpdated. {0}. Data: {1}", _guid, data);
            _eventAppToolsChange.OnNext(new EventAppToolsChange.EventData
            {
                ConnectionId = Context.ConnectionId,
                Data = data
            });
            return ResponseData<string>.Success(data, string.Empty).TaskFromResult<IResponseData<string>>();
        }

        public Task<IResponseData<string>> OnListResourcesUpdated(string data)
        {
            _logger.LogTrace("RemoteApp OnListResourcesUpdated. {0}. Data: {1}", _guid, data);
            // _onListResourcesUpdated.OnNext(Unit.Default);
            return ResponseData<string>.Success(data, string.Empty).TaskFromResult<IResponseData<string>>();
        }

        public Task<IResponseData<string>> OnDomainReloadStarted(string data)
        {
            _logger.LogInformation("RemoteApp OnDomainReloadStarted. {0}. Data: {1}", _guid, data);
            _requestTrackingService.NotifyDomainReload(Context.ConnectionId);
            return ResponseData<string>.Success(data, string.Empty).TaskFromResult<IResponseData<string>>();
        }

        public Task<IResponseData<string>> OnDomainReloadCompleted(DomainReloadCompletedData data)
        {
            _logger.LogInformation("RemoteApp OnDomainReloadCompleted. {0}. ConnectionId: {1}, PendingRequests: {2}",
                _guid, data.ConnectionId, data.PendingRequestIds?.Length ?? 0);

            if (data.PendingRequestIds != null && data.PendingRequestIds.Length > 0)
            {
                _requestTrackingService.RegisterPendingRequests(Context.ConnectionId, data.PendingRequestIds);
            }

            return ResponseData<string>.Success(string.Empty, string.Empty).TaskFromResult<IResponseData<string>>();
        }

        public Task<IResponseData<string>> OnToolRequestCompleted(string requestId, string responseJson)
        {
            _logger.LogTrace("RemoteApp OnToolRequestCompleted. {0}. RequestId: {1}", _guid, requestId);

            try
            {
                var response = System.Text.Json.JsonSerializer.Deserialize<IResponseData<object>>(responseJson, CreateJsonOptions());
                if (response != null)
                {
                    _requestTrackingService.CompleteRequest(requestId, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing tool response for RequestId: {RequestId}", requestId);
            }

            return ResponseData<string>.Success(string.Empty, string.Empty).TaskFromResult<IResponseData<string>>();
        }

        public Task<IResponseData<string>> OnResourceRequestCompleted(string requestId, string responseJson)
        {
            _logger.LogTrace("RemoteApp OnResourceRequestCompleted. {0}. RequestId: {1}", _guid, requestId);

            try
            {
                var response = System.Text.Json.JsonSerializer.Deserialize<IResponseData<object>>(responseJson, CreateJsonOptions());
                if (response != null)
                {
                    _requestTrackingService.CompleteRequest(requestId, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing resource response for RequestId: {RequestId}", requestId);
            }

            return ResponseData<string>.Success(string.Empty, string.Empty).TaskFromResult<IResponseData<string>>();
        }
    }
}
