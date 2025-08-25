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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.Unity.MCP.Common;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using com.IvanMurzak.Unity.MCP.Common.Json;
using Microsoft.AspNetCore.Http;

namespace com.IvanMurzak.Unity.MCP.Server
{
    using Consts = Common.Consts;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure NLog
            var nLog = LogManager.Setup().LoadConfigurationFromFile("NLog.config");
            var logger = nLog.GetCurrentClassLogger();
            try
            {
                var dataArguments = new DataArguments(args);

                if (dataArguments.PluginPort == dataArguments.ClientPort)
                {
                    throw new ArgumentException($"Plugin port ({dataArguments.PluginPort}) and client port ({dataArguments.ClientPort}) cannot be the same.");
                }

                // TODO: remove usage of static ConnectionConfig, replace it with instance with DI injection.
                // Set the runtime configurable timeout
                ConnectionConfig.TimeoutMs = dataArguments.PluginTimeoutMs;

                var consoleWriteLine = dataArguments.ClientTransport switch
                {
                    Consts.MCP.Server.TransportMethod.stdio => (Action<string>)(message => Console.Error.WriteLine(message)),
                    Consts.MCP.Server.TransportMethod.http => (Action<string>)(message => Console.WriteLine(message)),
                    _ => throw new ArgumentException($"Unsupported transport method: {dataArguments.ClientTransport}. " +
                        $"Supported methods are: {Consts.MCP.Server.TransportMethod.stdio}, {Consts.MCP.Server.TransportMethod.http}")
                };

                consoleWriteLine("Location: " + Environment.CurrentDirectory);
                consoleWriteLine($"Launch arguments: {string.Join(" ", args)}");
                consoleWriteLine($"Parsed arguments: {JsonSerializer.Serialize(dataArguments, JsonOptions.Pretty)}");

                var builder = WebApplication.CreateBuilder(args);

                // Replace default logging with NLog
                // builder.Logging.ClearProviders();
                builder.Logging.AddNLog();

                // Setup SignalR ---------------------------------------------------------------

                builder.Services.AddSignalR(configure =>
                {
                    configure.EnableDetailedErrors = true;
                    configure.MaximumReceiveMessageSize = 1024 * 1024 * 256; // 256 MB
                    configure.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                    configure.KeepAliveInterval = TimeSpan.FromSeconds(10);
                    configure.HandshakeTimeout = TimeSpan.FromSeconds(15);
                });

                // Setup MCP Plugin ---------------------------------------------------------------
                builder.Services.AddMcpPlugin(logger: new ConsoleLogger("McpPlugin"), configure =>
                {
                    configure
                        .WithServerFeatures(dataArguments)
                        .AddLogging(logging =>
                        {
                            logging.AddNLog();
                            logging.SetMinimumLevel(LogLevel.Debug);
                        });
                }).Build(new Reflector());

                // Setup MCP Server ---------------------------------------------------------------

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

                if (dataArguments.ClientTransport == Consts.MCP.Server.TransportMethod.stdio)
                {
                    // Configure all logs to go to stderr. This is needed for MCP STDIO server to work properly.
                    builder.Logging.AddConsole(consoleLogOptions => consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace);

                    // Configure STDIO transport
                    mcpBuilder = mcpBuilder.WithStdioServerTransport();
                }
                else if (dataArguments.ClientTransport == Consts.MCP.Server.TransportMethod.http)
                {
                    // Configure HTTP transport
                    mcpBuilder = mcpBuilder.WithHttpTransport(options =>
                    {
                        options.RunSessionHandler = async (context, server, cancellationToken) =>
                        {
                            try
                            {
                                // This is where you can run logic before a session starts
                                // For example, you can log the session start or initialize resources
                                logger.Debug("Running session handler for HTTP transport.");

                                var service = new McpServerService(
                                    server.Services!.GetRequiredService<ILogger<McpServerService>>(),
                                    server,
                                    server.Services!.GetRequiredService<IMcpRunner>(),
                                    server.Services!.GetRequiredService<IToolRunner>(),
                                    server.Services!.GetRequiredService<IResourceRunner>(),
                                    server.Services!.GetRequiredService<EventAppToolsChange>()
                                );

                                try
                                {
                                    await service.StartAsync(cancellationToken);
                                    await server.RunAsync(cancellationToken);
                                }
                                finally
                                {
                                    await service.StopAsync(cancellationToken);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, "Error occurred while processing HTTP transport session.");
                            }
                        };
                    });
                }
                else
                {
                    throw new ArgumentException($"Unsupported transport method: {dataArguments.ClientTransport}. " +
                        $"Supported methods are: {Consts.MCP.Server.TransportMethod.stdio}, {Consts.MCP.Server.TransportMethod.http}");
                }

                // builder.WebHost.UseUrls(Consts.Hub.DefaultEndpoint);

                builder.WebHost.UseKestrel(options =>
                {
                    logger.Info($"Start listening on port: {dataArguments.PluginPort}");
                    options.ListenAnyIP(dataArguments.PluginPort);

                    if (dataArguments.ClientTransport == Consts.MCP.Server.TransportMethod.http)
                    {
                        logger.Info($"Start listening on port: {dataArguments.ClientPort}");
                        options.ListenAnyIP(dataArguments.ClientPort);
                    }
                });

                var app = builder.Build();

                // Middleware ----------------------------------------------------------------
                // ---------------------------------------------------------------------------

                // Setup SignalR ----------------------------------------------------
                app.UseRouting();
                app.MapHub<RemoteApp>(Consts.Hub.RemoteApp, options =>
                {
                    options.Transports = HttpTransports.All;
                    options.ApplicationMaxBufferSize = 1024 * 1024 * 10; // 10 MB
                    options.TransportMaxBufferSize = 1024 * 1024 * 10; // 10 MB
                });

                // Setup MCP client -------------------------------------------------
                if (dataArguments.ClientTransport == Consts.MCP.Server.TransportMethod.http)
                {
                    // app.MapGet("/", context =>
                    // {
                    //     context.Response.Redirect("/mcp", permanent: false);
                    //     return Task.CompletedTask;
                    // });
                    app.MapGet("/", () =>
                    {
                        var header =
                            "Author: Ivan Murzak (https://github.com/IvanMurzak)\n" +
                            "Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)\n" +
                            "Copyright (c) 2025 Ivan Murzak\n" +
                            "Licensed under the Apache License, Version 2.0.\n" +
                            "See the LICENSE file in the project root for more information.\n";
                        return Results.Text(header, Consts.MimeType.TextPlain);
                    });
                    app.MapMcp("mcp");
                }

                // Print logs -------------------------------------------------------
                if (logger.IsEnabled(NLog.LogLevel.Debug))
                {
                    var endpointDataSource = app.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
                    foreach (var endpoint in endpointDataSource.Endpoints)
                        logger.Debug($"Configured endpoint: {endpoint.DisplayName}");

                    app.Use(async (context, next) =>
                    {
                        logger.Debug($"Request: {context.Request.Method} {context.Request.Path}");
                        try
                        {
                            await next.Invoke();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, $"Error occurred while processing request: {context.Request.Method} {context.Request.Path}");
                            return;
                        }
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
