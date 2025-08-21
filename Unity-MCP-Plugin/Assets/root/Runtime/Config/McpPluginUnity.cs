using System;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using R3;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP
{
    public partial class McpPluginUnity
    {
        Data data = new Data();
        static event Action<Data> onChanged;

        static McpPluginUnity instance;
        static McpPluginUnity Instance
        {
            get
            {
                Init();
                return instance;
            }
        }

        public static void Init()
        {
            if (instance == null)
            {
                instance = GetOrCreateInstance(out var wasCreated);
                if (instance == null)
                {
                    Debug.LogWarning("[McpPluginUnity] ConnectionConfig instance is null");
                    return;
                }
                else if (wasCreated)
                {
                    Save();
                }
            }
        }

        public static bool IsLogActive(LogLevel level)
            => (Instance.data ??= new Data()).LogLevel.IsActive(level);

        public static LogLevel LogLevel
        {
            get => Instance.data?.LogLevel ?? LogLevel.Trace;
            set
            {
                Instance.data ??= new Data();
                Instance.data.LogLevel = value;
                NotifyChanged(Instance.data);
            }
        }
        public static string Host
        {
            get => Instance.data?.Host ?? Data.DefaultHost;
            set
            {
                Instance.data ??= new Data();
                Instance.data.Host = value;
                NotifyChanged(Instance.data);
            }
        }
        public static int Port
        {
            get
            {
                if (Uri.TryCreate(Host, UriKind.Absolute, out var uri) && uri.Port > 0)
                    return uri.Port;

                return Consts.Hub.DefaultPort;
            }
        }
        public static bool KeepConnected
        {
            get => Instance.data?.KeepConnected ?? true;
            set
            {
                Instance.data ??= new Data();
                Instance.data.KeepConnected = value;
                NotifyChanged(Instance.data);
            }
        }
        public static int TimeoutMs
        {
            get => Instance.data?.TimeoutMs ?? Consts.Hub.DefaultTimeoutMs;
            set
            {
                Instance.data ??= new Data();
                Instance.data.TimeoutMs = value;
                NotifyChanged(Instance.data);
            }
        }
        public static ReadOnlyReactiveProperty<HubConnectionState> ConnectionState
            => McpPlugin.Instance.ConnectionState;

        public static ReadOnlyReactiveProperty<bool> IsConnected => McpPlugin.Instance.ConnectionState
            .Select(x => x == HubConnectionState.Connected)
            .ToReadOnlyReactiveProperty(false);

        public static void Validate()
        {
            var changed = false;
            var data = Instance.data ??= new Data();

            if (data.Port < 0 || data.Port > Consts.Hub.MaxPort)
            {
                data.Port = Consts.Hub.DefaultPort;
                changed = true;
            }

            if (string.IsNullOrEmpty(data.Host))
            {
                data.Host = Data.DefaultHost;
                changed = true;
            }

            if (changed)
                NotifyChanged(data);
        }

        public static void SubscribeOnChanged(Action<Data> action)
        {
            if (action == null)
                return;

            onChanged += action;
            Safe.Run(action, Instance.data, logLevel: Instance.data?.LogLevel ?? LogLevel.Trace);
        }
        public static void UnsubscribeOnChanged(Action<Data> action)
        {
            if (action == null)
                return;

            onChanged -= action;
        }

        static void NotifyChanged(Data data)
            => Safe.Run(onChanged, data, logLevel: data?.LogLevel ?? LogLevel.Trace);
    }
}