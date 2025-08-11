using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.ReflectorNet.Model
{
    public static class ResponseCallToolExtensions
    {
        public static ResponseCallTool Log(this ResponseCallTool target, ILogger logger, Exception? ex = null)
        {
            if (target.IsError)
                logger.LogError(ex, $"Response to AI:\n{target.Content.FirstOrDefault()?.Text}");
            else
                logger.LogInformation(ex, $"Response to AI:\n{target.Content.FirstOrDefault()?.Text}");

            return target;
        }

        public static IResponseData<ResponseCallTool> Pack(this ResponseCallTool target, string requestId, string? message = null)
        {
            if (target.IsError)
                return ResponseData<ResponseCallTool>.Error(requestId, message ?? target.Content.FirstOrDefault()?.Text ?? "Tool execution error.")
                    .SetData(target);
            else
                return ResponseData<ResponseCallTool>.Success(requestId, message ?? target.Content.FirstOrDefault()?.Text ?? "Tool executed successfully.")
                    .SetData(target);
        }
    }
}