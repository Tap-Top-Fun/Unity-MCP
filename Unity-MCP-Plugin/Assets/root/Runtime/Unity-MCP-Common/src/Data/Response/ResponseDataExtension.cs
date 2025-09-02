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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Common.Model
{
    public static class ResponseDataExtension
    {
        public static IResponseData<T> Log<T>(this IResponseData<T> response, ILogger logger, Exception? ex = null)
        {
            if (response.Status == ResponseStatus.Error)
                logger.LogError(ex, response.Message ?? "Execution failed.");
            else if (response.Status == ResponseStatus.Success)
                logger.LogInformation(ex, response.Message ?? "Executed successfully.");
            else if (response.Status == ResponseStatus.Processing)
                logger.LogInformation(ex, response.Message ?? "Execution is still processing.");

            return response;
        }
        public static IResponseData<T> SetData<T>(this IResponseData<T> response, T? data)
        {
            response.Value = data;
            return response;
        }
        public static IResponseData<T> SetError<T>(this IResponseData<T> response, string? message = null)
        {
            response.Status = ResponseStatus.Error;
            response.Message = message ?? "Execution failed.";
            return response;
        }
        public static IResponseData<T> SetSuccess<T>(this IResponseData<T> response, string? message = null)
        {
            response.Status = ResponseStatus.Success;
            response.Message = message ?? "Executed successfully.";
            return response;
        }
        public static IResponseData<T> SetProcessing<T>(this IResponseData<T> response, string? message = null)
        {
            response.Status = ResponseStatus.Processing;
            response.Message = message ?? "Execution is still processing.";
            return response;
        }

        public static Task<IResponseData<T>> TaskFromResult<T>(this IResponseData<T> response)
            => Task.FromResult(response);
    }
}
