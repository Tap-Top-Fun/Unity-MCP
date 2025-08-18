using System;
using System.Collections.Generic;
using com.IvanMurzak.Unity.MCP.Common;

namespace com.IvanMurzak.Unity.MCP.Server
{
    public class DataArguments
    {
        public int Port { get; private set; }
        public int TimeoutMs { get; private set; }
        public Consts.MCP.Server.TransportMethod Transport { get; private set; }

        public DataArguments(string[] args)
        {
            Port = Consts.Hub.DefaultPort;
            TimeoutMs = Consts.Hub.DefaultTimeoutMs;
            Transport = Consts.MCP.Server.TransportMethod.stdio;

            ParseEnvironmentVariables(); // env variables - second priority
            ParseCommandLineArguments(args); // command line args - first priority (override previous values)
        }

        void ParseEnvironmentVariables()
        {
            var envPort = Environment.GetEnvironmentVariable(Consts.MCP.Server.Env.Port);
            if (envPort != null && int.TryParse(envPort, out var parsedEnvPort))
                Port = parsedEnvPort;

            var envTimeout = Environment.GetEnvironmentVariable(Consts.MCP.Server.Env.Timeout);
            if (envTimeout != null && int.TryParse(envTimeout, out var parsedEnvTimeoutMs))
                TimeoutMs = parsedEnvTimeoutMs;

            var envTransport = Environment.GetEnvironmentVariable(Consts.MCP.Server.Env.TransportMethod);
            if (envTransport != null && Enum.TryParse(envTransport, out Consts.MCP.Server.TransportMethod parsedEnvTransport))
                Transport = parsedEnvTransport;
        }
        void ParseCommandLineArguments(string[] args)
        {
            var commandLineArgs = ArgsUtils.ParseLineArguments(args);

            var argPort = commandLineArgs.GetValueOrDefault(Consts.MCP.Server.Args.Port.TrimStart('-'));
            if (argPort != null && int.TryParse(argPort, out var port))
                Port = port;

            var argTimeout = commandLineArgs.GetValueOrDefault(Consts.MCP.Server.Args.Timeout.TrimStart('-'));
            if (argTimeout != null && int.TryParse(argTimeout, out var timeoutMs))
                TimeoutMs = timeoutMs;

            var argTransport = commandLineArgs.GetValueOrDefault(Consts.MCP.Server.Args.TransportMethod.TrimStart('-'));
            if (argTransport != null && Enum.TryParse(argTransport, out Consts.MCP.Server.TransportMethod parsedArgTransport))
                Transport = parsedArgTransport;
        }
    }
}