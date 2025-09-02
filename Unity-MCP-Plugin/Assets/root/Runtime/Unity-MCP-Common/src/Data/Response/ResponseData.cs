/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/
#nullable enable
using System;

namespace com.IvanMurzak.Unity.MCP.Common.Model
{
    public class ResponseData<T> : IResponseData<T>
    {
        public string RequestID { get; set; } = string.Empty;
        public ResponseStatus Status { get; set; }
        public string? Message { get; set; }
        public T? Value { get; set; }

        public ResponseData() { }
        public ResponseData(string requestId, ResponseStatus status)
        {
            RequestID = requestId ?? throw new ArgumentNullException(nameof(requestId));
            Status = status;
        }

        public static ResponseData<T> Success(string requestId, string? message = null) => new ResponseData<T>(requestId, ResponseStatus.Success)
        {
            Message = message
        };
        public static ResponseData<T> Error(string requestId, string? message = null) => new ResponseData<T>(requestId, ResponseStatus.Error)
        {
            Message = message
        };
        public static ResponseData<T> Processing(string requestId, string? message = null) => new ResponseData<T>(requestId, ResponseStatus.Processing)
        {
            Message = message
        };
    }
}
