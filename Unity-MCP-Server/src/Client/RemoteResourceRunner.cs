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
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using R3;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public class RemoteResourceRunner : IResourceRunner, IDisposable
    {
        readonly ILogger _logger;
        readonly IHubContext<RemoteApp> _remoteAppContext;
        readonly IRequestTrackingService _requestTrackingService;
        readonly CancellationTokenSource cts = new();
        readonly CompositeDisposable _disposables = new();

        public RemoteResourceRunner(ILogger<RemoteResourceRunner> logger, IHubContext<RemoteApp> remoteAppContext, IRequestTrackingService requestTrackingService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogTrace("Ctor.");
            _remoteAppContext = remoteAppContext ?? throw new ArgumentNullException(nameof(remoteAppContext));
            _requestTrackingService = requestTrackingService ?? throw new ArgumentNullException(nameof(requestTrackingService));
        }

        public Task<IResponseData<ResponseResourceContent[]>> RunResourceContent(IRequestResourceContent requestData, CancellationToken cancellationToken = default)
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            
            return _requestTrackingService.TrackRequestAsync(
                requestData.RequestID,
                async () =>
                {
                    var response = await ClientUtils.InvokeAsync<IRequestResourceContent, ResponseResourceContent[], RemoteApp>(
                        logger: _logger,
                        hubContext: _remoteAppContext,
                        methodName: Consts.RPC.Client.RunResourceContent,
                        requestData: requestData,
                        cancellationToken: linkedCts.Token);

                    if (response.IsError)
                        return ResponseData<ResponseResourceContent[]>.Error(requestData.RequestID, response.Message ?? "Got an error during invoking resource");

                    return response;
                },
                ConnectionConfig.TimeoutMs,
                linkedCts.Token);
        }

        public Task<IResponseData<ResponseListResource[]>> RunListResources(IRequestListResources requestData, CancellationToken cancellationToken = default)
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            
            return _requestTrackingService.TrackRequestAsync(
                requestData.RequestID,
                async () =>
                {
                    var response = await ClientUtils.InvokeAsync<IRequestListResources, ResponseListResource[], RemoteApp>(
                        logger: _logger,
                        hubContext: _remoteAppContext,
                        methodName: Consts.RPC.Client.RunListResources,
                        requestData: requestData,
                        cancellationToken: linkedCts.Token);

                    if (response.IsError)
                        return ResponseData<ResponseListResource[]>.Error(requestData.RequestID, response.Message ?? "Got an error during invoking resource");

                    return response;
                },
                ConnectionConfig.TimeoutMs,
                linkedCts.Token);
        }

        public Task<IResponseData<ResponseResourceTemplate[]>> RunResourceTemplates(IRequestListResourceTemplates requestData, CancellationToken cancellationToken = default)
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
            
            return _requestTrackingService.TrackRequestAsync(
                requestData.RequestID,
                async () =>
                {
                    var response = await ClientUtils.InvokeAsync<IRequestListResourceTemplates, ResponseResourceTemplate[], RemoteApp>(
                        logger: _logger,
                        hubContext: _remoteAppContext,
                        methodName: Consts.RPC.Client.RunListResourceTemplates,
                        requestData: requestData,
                        cancellationToken: linkedCts.Token);

                    if (response.IsError)
                        return ResponseData<ResponseResourceTemplate[]>.Error(requestData.RequestID, response.Message ?? "Got an error during invoking resource");

                    return response;
                },
                ConnectionConfig.TimeoutMs,
                linkedCts.Token);
        }

        public void Dispose()
        {
            _logger.LogTrace("Dispose.");
            _disposables.Dispose();

            if (!cts.IsCancellationRequested)
                cts.Cancel();

            cts.Dispose();
        }
    }
}
