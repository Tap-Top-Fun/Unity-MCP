using System;
using System.Threading.Tasks;

namespace com.IvanMurzak.Unity.MCP.Common.Server
{
    public interface IServerCommand<TRequest, TResponse> : IDisposable
    {
        string Class { get; }
        string? Method { get; }
        Task<TResponse> Call(Action<TRequest> configCommand);
    }
}