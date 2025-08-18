using com.IvanMurzak.Unity.MCP.Common;
using Microsoft.Extensions.DependencyInjection;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public static class ExtensionsMcpServerBuilder
    {
        public static IMcpPluginBuilder WithServerFeatures(this IMcpPluginBuilder builder, DataArguments dataArguments)
        {
            builder.Services.AddRouting();
            if (dataArguments.Transport == Consts.MCP.Server.TransportMethod.stdio)
                builder.Services.AddHostedService<McpServerService>();

            builder.Services.AddSingleton<EventAppToolsChange>();
            builder.Services.AddSingleton<IToolRunner, RemoteToolRunner>();
            builder.Services.AddSingleton<IResourceRunner, RemoteResourceRunner>();

            builder.AddMcpRunner();

            return builder;
        }
    }
}
