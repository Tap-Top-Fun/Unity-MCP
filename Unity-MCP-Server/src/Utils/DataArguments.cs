using System;
using System.Collections.Generic;
using com.IvanMurzak.Unity.MCP.Common;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public class DataArguments
    {
        public int PluginPort { get; private set; }
        public int PluginTimeoutMs { get; private set; }

        public int ClientPort { get; private set; } = 80;
        public Consts.MCP.Server.TransportMethod ClientTransport { get; private set; }

        public DataArguments(string[] args)
        {
            PluginPort = Consts.Hub.DefaultPort;
            PluginTimeoutMs = Consts.Hub.DefaultTimeoutMs;
            ClientTransport = Consts.MCP.Server.TransportMethod.stdio;

            ParseEnvironmentVariables(); // env variables - second priority
            ParseCommandLineArguments(args); // command line args - first priority (override previous values)
        }

        void ParseEnvironmentVariables()
        {
            // --- Plugin variables ---

            var envPluginPort = Environment.GetEnvironmentVariable(Consts.MCP.Server.Env.PluginPort);
            if (envPluginPort != null && int.TryParse(envPluginPort, out var parsedEnvPort))
                PluginPort = parsedEnvPort;

            var envPluginTimeout = Environment.GetEnvironmentVariable(Consts.MCP.Server.Env.PluginTimeout);
            if (envPluginTimeout != null && int.TryParse(envPluginTimeout, out var parsedEnvTimeoutMs))
                PluginTimeoutMs = parsedEnvTimeoutMs;

            // --- Client variables ---

            var envClientPort = Environment.GetEnvironmentVariable(Consts.MCP.Server.Env.ClientPort);
            if (envClientPort != null && int.TryParse(envClientPort, out var parsedEnvClientPort))
                ClientPort = parsedEnvClientPort;

            var envClientTransport = Environment.GetEnvironmentVariable(Consts.MCP.Server.Env.ClientTransportMethod);
            if (envClientTransport != null && Enum.TryParse(envClientTransport, out Consts.MCP.Server.TransportMethod parsedEnvTransport))
                ClientTransport = parsedEnvTransport;
        }
        void ParseCommandLineArguments(string[] args)
        {
            var commandLineArgs = ArgsUtils.ParseLineArguments(args);

            // --- Plugin variables ---

            var argPluginPort = commandLineArgs.GetValueOrDefault(Consts.MCP.Server.Args.PluginPort.TrimStart('-'));
            if (argPluginPort != null && int.TryParse(argPluginPort, out var port))
                PluginPort = port;

            var argPluginTimeout = commandLineArgs.GetValueOrDefault(Consts.MCP.Server.Args.PluginTimeout.TrimStart('-'));
            if (argPluginTimeout != null && int.TryParse(argPluginTimeout, out var timeoutMs))
                PluginTimeoutMs = timeoutMs;

            // --- Client variables ---

            var argClientPort = commandLineArgs.GetValueOrDefault(Consts.MCP.Server.Args.ClientPort.TrimStart('-'));
            if (argClientPort != null && int.TryParse(argClientPort, out var clientPort))
                ClientPort = clientPort;

            var argClientTransport = commandLineArgs.GetValueOrDefault(Consts.MCP.Server.Args.ClientTransportMethod.TrimStart('-'));
            if (argClientTransport != null && Enum.TryParse(argClientTransport, out Consts.MCP.Server.TransportMethod parsedArgTransport))
                ClientTransport = parsedArgTransport;
        }
    }
}