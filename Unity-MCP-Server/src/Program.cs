using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using com.IvanMurzak.Unity.MCP.Common;
using NLog.Extensions.Logging;
using NLog;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Hosting;
using com.IvanMurzak.ReflectorNet;
using ModelContextProtocol.Server;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using System.Text.Json;
using com.IvanMurzak.Unity.MCP.Server.Utils;

namespace com.IvanMurzak.Unity.MCP.Server
{
    using Consts = Common.Consts;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure NLog
            var logger = LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();
            try
            {
                var dataArguments = new DataArguments(args);

                // TODO: remove usage of static ConnectionConfig, replace it with instance with DI injection.
                // Set the runtime configurable timeout
                ConnectionConfig.TimeoutMs = dataArguments.TimeoutMs;

                var consoleWriteLine = dataArguments.Transport switch
                {
                    Consts.MCP.Server.TransportMethod.stdio => (Action<string>)(message => Console.Error.WriteLine(message)),
                    Consts.MCP.Server.TransportMethod.http => (Action<string>)(message => Console.WriteLine(message)),
                    _ => throw new ArgumentException($"Unsupported transport method: {dataArguments.Transport}. " +
                        $"Supported methods are: {Consts.MCP.Server.TransportMethod.stdio}, {Consts.MCP.Server.TransportMethod.http}")
                };

                consoleWriteLine("Location: " + Environment.CurrentDirectory);
                consoleWriteLine($"Launch arguments: {string.Join(", ", args)}");
                consoleWriteLine($"Parsed arguments: {JsonSerializer.Serialize(dataArguments, JsonOptions.Pretty)}");

                var builder = WebApplication.CreateBuilder(args);

                // Replace default logging with NLog
                // builder.Logging.ClearProviders();
                builder.Logging.AddNLog();

                builder.Services.AddSignalR(configure =>
                {
                    configure.EnableDetailedErrors = true;
                    configure.MaximumReceiveMessageSize = 1024 * 1024 * 256; // 256 MB
                    configure.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                    configure.KeepAliveInterval = TimeSpan.FromSeconds(15);
                    configure.HandshakeTimeout = TimeSpan.FromSeconds(15);
                });

                // Setup MCP server ---------------------------------------------------------------

                var mcpBuilder = builder.Services
                    .AddMcpServer(options =>
                    {
                        options.Capabilities ??= new();
                        options.Capabilities.Tools ??= new();
                        options.Capabilities.Tools.ListChanged = true;
                    })
                    .WithToolsFromAssembly()
                    .WithCallToolHandler(ToolRouter.Call)
                    .WithListToolsHandler(ToolRouter.ListAll);

                // --- Additional handlers for the future implementation
                //.WithPromptsFromAssembly()
                //.WithReadResourceHandler(ResourceRouter.ReadResource)
                //.WithListResourcesHandler(ResourceRouter.ListResources)
                //.WithListResourceTemplatesHandler(ResourceRouter.ListResourceTemplates);
                // -----------------------------------------------------

                if (dataArguments.Transport == Consts.MCP.Server.TransportMethod.stdio)
                {
                    // Configure all logs to go to stderr. This is needed for MCP STDIO server to work properly.
                    builder.Logging.AddConsole(consoleLogOptions => consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace);

                    // Configure STDIO transport
                    mcpBuilder = mcpBuilder.WithStdioServerTransport();
                }
                else if (dataArguments.Transport == Consts.MCP.Server.TransportMethod.http)
                {
                    // builder.Services.AddSingleton<IMcpServer, McpServer>();
                    // Configure HTTP transport
                    mcpBuilder = mcpBuilder.WithHttpTransport();

                    // Still need to enable STDIO, because `WithHttpTransport` doesn't inject IMcpServer into di container
                    mcpBuilder = mcpBuilder.WithStdioServerTransport();
                }
                else
                {
                    throw new ArgumentException($"Unsupported transport method: {dataArguments.Transport}. " +
                        $"Supported methods are: {Consts.MCP.Server.TransportMethod.stdio}, {Consts.MCP.Server.TransportMethod.http}");
                }

                // Setup McpApp ----------------------------------------------------------------
                builder.Services.AddMcpPlugin(logger: null, configure =>
                {
                    configure
                        .WithServerFeatures()
                        .AddLogging(logging =>
                        {
                            logging.AddNLog();
                            logging.SetMinimumLevel(LogLevel.Debug);
                        });
                }).Build(new Reflector());

                // builder.WebHost.UseUrls(Consts.Hub.DefaultEndpoint);

                builder.WebHost.UseKestrel(options =>
                {
                    options.ListenLocalhost(dataArguments.Port);
                    options.ListenAnyIP(dataArguments.Port);
                });

                var app = builder.Build();

                // Middleware ----------------------------------------------------------------
                // ---------------------------------------------------------------------------

                app.UseRouting();
                app.MapHub<RemoteApp>(Consts.Hub.RemoteApp, options =>
                {
                    options.Transports = HttpTransports.All;
                    options.ApplicationMaxBufferSize = 1024 * 1024 * 10; // 10 MB
                    options.TransportMaxBufferSize = 1024 * 1024 * 10; // 10 MB
                });

                if (logger.IsEnabled(NLog.LogLevel.Debug))
                {
                    var endpointDataSource = app.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
                    foreach (var endpoint in endpointDataSource.Endpoints)
                        logger.Debug($"Configured endpoint: {endpoint.DisplayName}");

                    app.Use(async (context, next) =>
                    {
                        logger.Debug($"Request: {context.Request.Method} {context.Request.Path}");
                        await next.Invoke();
                        logger.Debug($"Response: {context.Response.StatusCode}");
                    });
                }

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Application stopped due to an exception.");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}
