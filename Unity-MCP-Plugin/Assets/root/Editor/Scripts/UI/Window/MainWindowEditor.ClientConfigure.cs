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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using R3;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.IvanMurzak.Unity.MCP.Editor
{
    using Consts = Common.Consts;

    public partial class MainWindowEditor : EditorWindow
    {
        string ProjectRootPath => Application.dataPath.EndsWith("/Assets")
            ? Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length)
            : Application.dataPath;

        void ConfigureClientsWindows(VisualElement root)
        {
            ConfigureClient(root.Query<VisualElement>("ConfigureClient-Claude-Desktop").First(),
                configPath: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Claude",
                    "claude_desktop_config.json"
                ),
                bodyPath: Consts.MCP.Server.DefaultBodyPath);

            ConfigureClient(root.Query<VisualElement>("ConfigureClient-Claude-Code").First(),
                configPath: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".claude.json"
                ),
                bodyPath: $"projects{Consts.MCP.Server.BodyPathDelimiter}"
                    + $"{ProjectRootPath}{Consts.MCP.Server.BodyPathDelimiter}"
                    + Consts.MCP.Server.DefaultBodyPath);

            ConfigureClient(root.Query<VisualElement>("ConfigureClient-VS-Code").First(),
                configPath: Path.Combine(
                    ".vscode",
                    "mcp.json"
                ),
                bodyPath: "servers");

            ConfigureClient(root.Query<VisualElement>("ConfigureClient-Cursor").First(),
                configPath: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".cursor",
                    "mcp.json"
                ),
                bodyPath: Consts.MCP.Server.DefaultBodyPath);
        }

        void ConfigureClientsMacAndLinux(VisualElement root)
        {
            ConfigureClient(root.Query<VisualElement>("ConfigureClient-Claude-Desktop").First(),
                configPath: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library",
                    "Application Support",
                    "Claude",
                    "claude_desktop_config.json"
                ),
                bodyPath: Consts.MCP.Server.DefaultBodyPath);

            ConfigureClient(root.Query<VisualElement>("ConfigureClient-Claude-Code").First(),
                configPath: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".claude.json"
                ),
                bodyPath: $"projects{Consts.MCP.Server.BodyPathDelimiter}"
                    + $"{ProjectRootPath}{Consts.MCP.Server.BodyPathDelimiter}"
                    + Consts.MCP.Server.DefaultBodyPath);

            ConfigureClient(root.Query<VisualElement>("ConfigureClient-VS-Code").First(),
                configPath: Path.Combine(
                    ".vscode",
                    "mcp.json"
                ),
                bodyPath: "servers");

            ConfigureClient(root.Query<VisualElement>("ConfigureClient-Cursor").First(),
                configPath: Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".cursor",
                    "mcp.json"
                ),
                bodyPath: Consts.MCP.Server.DefaultBodyPath);
        }

        void ConfigureClient(VisualElement root, string configPath, string bodyPath = Consts.MCP.Server.DefaultBodyPath)
        {
            var statusCircle = root.Query<VisualElement>("configureStatusCircle").First();
            var statusText = root.Query<Label>("configureStatusText").First();
            var btnConfigure = root.Query<Button>("btnConfigure").First();

            var isConfiguredResult = IsMcpClientConfigured(configPath, bodyPath);

            statusCircle.RemoveFromClassList(USS_IndicatorClass_Connected);
            statusCircle.RemoveFromClassList(USS_IndicatorClass_Connecting);
            statusCircle.RemoveFromClassList(USS_IndicatorClass_Disconnected);

            statusCircle.AddToClassList(isConfiguredResult
                ? USS_IndicatorClass_Connected
                : USS_IndicatorClass_Disconnected);

            statusText.text = isConfiguredResult ? "Configured" : "Not Configured";
            btnConfigure.text = isConfiguredResult ? "Reconfigure" : "Configure";

            btnConfigure.RegisterCallback<ClickEvent>(evt =>
            {
                var configureResult = ConfigureMcpClient(configPath, bodyPath);

                statusText.text = configureResult ? "Configured" : "Not Configured";

                statusCircle.RemoveFromClassList(USS_IndicatorClass_Connected);
                statusCircle.RemoveFromClassList(USS_IndicatorClass_Connecting);
                statusCircle.RemoveFromClassList(USS_IndicatorClass_Disconnected);

                statusCircle.AddToClassList(configureResult
                    ? USS_IndicatorClass_Connected
                    : USS_IndicatorClass_Disconnected);

                btnConfigure.text = configureResult ? "Reconfigure" : "Configure";
            });
        }

        bool IsMcpClientConfigured(string configPath, string bodyPath = Consts.MCP.Server.DefaultBodyPath)
        {
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
                return false;

            try
            {
                var json = File.ReadAllText(configPath);

                if (string.IsNullOrWhiteSpace(json))
                    return false;

                var rootObj = JsonNode.Parse(json)?.AsObject();
                if (rootObj == null)
                    return false;

                var pathSegments = bodyPath.Split(new[] { '.', '/' }, StringSplitOptions.RemoveEmptyEntries);

                var mcpServers = rootObj[bodyPath]?.AsObject();
                if (mcpServers == null)
                    return false;

                foreach (var kv in mcpServers)
                {
                    var command = kv.Value?["command"]?.GetValue<string>();
                    if (string.IsNullOrEmpty(command) || !IsCommandMatch(command!))
                        continue;

                    var args = kv.Value?["args"]?.AsArray();
                    return DoArgumentsMatch(args);
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading config file: {ex.Message}");
                Debug.LogException(ex);
                return false;
            }
        }

        bool IsCommandMatch(string command)
        {
            // Normalize both paths for comparison
            try
            {
                var normalizedCommand = Path.GetFullPath(command.Replace('/', Path.DirectorySeparatorChar));
                var normalizedTarget = Path.GetFullPath(Startup.Server.ExecutableFullPath.Replace('/', Path.DirectorySeparatorChar));
                return string.Equals(normalizedCommand, normalizedTarget, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // If normalization fails, fallback to string comparison
                return string.Equals(command, Startup.Server.ExecutableFullPath, StringComparison.OrdinalIgnoreCase);
            }
        }

        bool DoArgumentsMatch(JsonArray? args)
        {
            if (args == null)
                return false;

            var targetPort = McpPluginUnity.Port.ToString();
            var targetTimeout = McpPluginUnity.TimeoutMs.ToString();

            var foundPort = false;
            var foundTimeout = false;

            // Check for both positional and named argument formats
            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i]?.GetValue<string>();
                if (string.IsNullOrEmpty(arg))
                    continue;

                // Check positional format
                if (i == 0 && arg == targetPort)
                    foundPort = true;
                else if (i == 1 && arg == targetTimeout)
                    foundTimeout = true;

                // Check named format
                else if (arg!.StartsWith($"{Consts.MCP.Server.Args.Port}=") && arg.Substring(Consts.MCP.Server.Args.Port.Length + 1) == targetPort)
                    foundPort = true;
                else if (arg!.StartsWith($"{Consts.MCP.Server.Args.PluginTimeout}=") && arg.Substring(Consts.MCP.Server.Args.PluginTimeout.Length + 1) == targetTimeout)
                    foundTimeout = true;
            }

            return foundPort && foundTimeout;
        }

        bool ConfigureMcpClient(string configPath, string bodyPath = Consts.MCP.Server.DefaultBodyPath)
        {
            if (string.IsNullOrEmpty(configPath))
                return false;

            Debug.Log($"{Consts.Log.Tag} Configuring MCP client with path: {configPath} and bodyPath: {bodyPath}");

            try
            {
                if (!File.Exists(configPath))
                {
                    // Create all necessary directories
                    Directory.CreateDirectory(Path.GetDirectoryName(configPath));

                    // Create the file if it doesn't exist
                    File.WriteAllText(
                        path: configPath,
                        contents: Startup.Server.RawJsonConfiguration(McpPluginUnity.Port, bodyPath, McpPluginUnity.TimeoutMs).ToString());
                    return true;
                }

                var json = File.ReadAllText(configPath);
                JsonObject? rootObj = null;

                try
                {
                    // Parse the existing config as JsonObject
                    rootObj = JsonNode.Parse(json)?.AsObject();
                    if (rootObj == null)
                        throw new Exception("Config file is not a valid JSON object.");
                }
                catch
                {
                    File.WriteAllText(
                        path: configPath,
                        contents: Startup.Server.RawJsonConfiguration(McpPluginUnity.Port, bodyPath, McpPluginUnity.TimeoutMs).ToString());
                    return true;
                }

                // Parse the injected config as JsonObject
                var pathSegments = Consts.MCP.Server.BodyPathSegments(bodyPath);
                var injectTarget = rootObj;

                foreach (var segment in pathSegments)
                {
                    var tempTarget = injectTarget[segment]?.AsObject();
                    if (tempTarget == null)
                    {
                        break;
                    }
                    injectTarget = tempTarget;
                }

                // Get mcpServers from both
                var mcpServers = rootObj[bodyPath]?.AsObject();

                var injectObj = Startup.Server.RawJsonConfiguration(McpPluginUnity.Port, pathSegments.Last(), McpPluginUnity.TimeoutMs);
                if (injectObj == null)
                    throw new Exception("Injected config is not a valid JSON object.");

                var injectMcpServers = injectObj[pathSegments.Last()]?.AsObject();
                if (injectMcpServers == null)
                    throw new Exception($"Missing '{pathSegments.Last()}' object in config.");

                if (mcpServers == null)
                {
                    // If mcpServers is null, create it
                    rootObj[bodyPath] = JsonNode.Parse(injectMcpServers.ToJsonString())?.AsObject();
                    File.WriteAllText(configPath, rootObj.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
                    return IsMcpClientConfigured(configPath, bodyPath);
                }

                // Find all command values in injectMcpServers
                var injectCommands = injectMcpServers
                    .Select(kv => kv.Value?["command"]?.GetValue<string>())
                    .Where(cmd => !string.IsNullOrEmpty(cmd))
                    .ToHashSet();

                // Remove any entry in mcpServers with a matching command
                var keysToRemove = mcpServers
                    .Where(kv => injectCommands.Contains(kv.Value?["command"]?.GetValue<string>()))
                    .Select(kv => kv.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                    mcpServers.Remove(key);

                // Merge/overwrite entries from injectMcpServers
                foreach (var kv in injectMcpServers)
                {
                    // Clone the value to avoid parent conflict
                    mcpServers[kv.Key] = kv.Value?.ToJsonString() is string jsonStr
                        ? JsonNode.Parse(jsonStr)
                        : null;
                }

                // Write back to file
                File.WriteAllText(configPath, rootObj.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

                return IsMcpClientConfigured(configPath, bodyPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading config file: {ex.Message}");
                Debug.LogException(ex);
                return false;
            }
        }
    }
}