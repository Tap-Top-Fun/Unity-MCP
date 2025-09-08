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

        public Task<IResponseData> OnListToolsUpdated(string data)
        {
            _logger.LogTrace("RemoteApp OnListToolsUpdated. {0}. Data: {1}", _guid, data);
            _eventAppToolsChange.OnNext(new EventAppToolsChange.EventData
            {
                ConnectionId = Context.ConnectionId,
                Data = data
            });
            return ResponseData.Success(data, string.Empty).TaskFromResult<IResponseData>();
        }

        public Task<IResponseData> OnListResourcesUpdated(string data)
        {
            _logger.LogTrace("RemoteApp OnListResourcesUpdated. {0}. Data: {1}", _guid, data);
            // _onListResourcesUpdated.OnNext(Unit.Default);
            return ResponseData.Success(data, string.Empty).TaskFromResult<IResponseData>();
        }

        public Task<IResponseData> OnToolRequestCompleted(ToolRequestCompletedData data)
        {
            _logger.LogTrace("RemoteApp OnToolRequestCompleted. {0}. RequestId: {1}", _guid, data.RequestId);

            try
            {
                _requestTrackingService.CompleteRequest(data.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing tool response for RequestId: {RequestId}", data.RequestId);
            }

            return ResponseData.Success(string.Empty, string.Empty).TaskFromResult<IResponseData>();
        }
    }
}
