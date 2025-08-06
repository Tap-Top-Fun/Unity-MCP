
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public interface IHubEndpointConnectionBuilder
    {
        Task<HubConnection> CreateConnectionAsync(string endpoint);
    }
}