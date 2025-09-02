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
using System.Threading.Tasks;
using com.IvanMurzak.ReflectorNet.Model;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public interface IRemoteApp : IToolResponseReceiver, IResourceResponseReceiver, IDisposable
    {
        Task<IResponseData<string>> OnListToolsUpdated(string data);
        Task<IResponseData<string>> OnListResourcesUpdated(string data);
        Task<IResponseData<string>> OnDomainReloadStarted(string data);
        Task<IResponseData<string>> OnDomainReloadCompleted(DomainReloadCompletedData data);
        Task<IResponseData<string>> OnToolRequestCompleted(string requestId, string responseJson);
        Task<IResponseData<string>> OnResourceRequestCompleted(string requestId, string responseJson);
    }

    public class DomainReloadCompletedData
    {
        public string[]? PendingRequestIds { get; set; }
        public string? ConnectionId { get; set; }
    }

    public interface IToolResponseReceiver
    {
        // Task RespondOnCallTool(IResponseData<IResponseCallTool> data, CancellationToken cancellationToken = default);
        // Task RespondOnListTool(IResponseData<List<IResponseListTool>> data, CancellationToken cancellationToken = default);
    }

    public interface IResourceResponseReceiver
    {
        // Task RespondOnResourceContent(IResponseData<List<IResponseResourceContent>> data, CancellationToken cancellationToken = default);
        // Task RespondOnListResources(IResponseData<List<IResponseListResource>> data, CancellationToken cancellationToken = default);
        // Task RespondOnListResourceTemplates(IResponseData<List<IResponseResourceTemplate>> data, CancellationToken cancellationToken = default);
    }
}
