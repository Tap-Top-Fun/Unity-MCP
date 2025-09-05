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
using System.Threading;
using System.Threading.Tasks;
using com.IvanMurzak.ReflectorNet.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using R3;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public class ConnectionManager : IConnectionManager
    {
        readonly string _guid = Guid.NewGuid().ToString();
        readonly ILogger<ConnectionManager> _logger;
        readonly ReactiveProperty<HubConnection?> _hubConnection = new();
        readonly IHubEndpointConnectionBuilder _hubConnectionBuilder;
        readonly ReactiveProperty<HubConnectionState> _connectionState = new(HubConnectionState.Disconnected);
        readonly ReactiveProperty<bool> _continueToReconnect = new(false);
        readonly CompositeDisposable _disposables = new();

        volatile Task<bool>? connectionTask;
        HubConnectionLogger? hubConnectionLogger;
        HubConnectionObservable? hubConnectionObservable;
        CancellationTokenSource? internalCts;
        public ReadOnlyReactiveProperty<HubConnectionState> ConnectionState => _connectionState.ToReadOnlyReactiveProperty();
        public ReadOnlyReactiveProperty<HubConnection?> HubConnection => _hubConnection.ToReadOnlyReactiveProperty();
        public ReadOnlyReactiveProperty<bool> KeepConnected => _continueToReconnect.ToReadOnlyReactiveProperty();
        public string Endpoint { get; set; } = string.Empty;

        public ConnectionManager(ILogger<ConnectionManager> logger, IHubEndpointConnectionBuilder hubConnectionBuilder)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogTrace("{0} Ctor.", _guid);

            _hubConnectionBuilder = hubConnectionBuilder ?? throw new ArgumentNullException(nameof(hubConnectionBuilder));
            _hubConnection
                .Subscribe(hubConnection =>
                {
                    if (hubConnection == null)
                    {
                        _connectionState.Value = HubConnectionState.Disconnected;
                        return;
                    }

                    hubConnection.ToObservable().State
                        .Subscribe(state => _connectionState.Value = state)
                        .AddTo(_disposables);
                })
                .AddTo(_disposables);

            _connectionState
                .Where(state => state == HubConnectionState.Reconnecting && _continueToReconnect.CurrentValue)
                .Subscribe(async state =>
                {
                    _logger.LogInformation("---------- CONNECT (ConnectionManager 1)");
                    await Connect(_disposables.ToCancellationToken());
                })
                .AddTo(_disposables);
        }

        public async Task InvokeAsync<TInput>(string methodName, TInput input, CancellationToken cancellationToken = default)
        {
            if (_hubConnection.CurrentValue?.State != HubConnectionState.Connected && _continueToReconnect.CurrentValue)
            {
                _logger.LogDebug("{0} Connection is not established. Attempting to connect...", _guid);

                // Attempt to connect if the connection is not established
                await Connect(cancellationToken);

                if (_hubConnection.CurrentValue?.State != HubConnectionState.Connected)
                {
                    _logger.LogError("{0} Can't establish connection with Remote.", _guid);
                    return;
                }
            }
            if (_hubConnection.CurrentValue == null)
            {
                _logger.LogError("{0} HubConnection is null. Can't invoke method {1}.", _guid, methodName);
                return;
            }

            await _hubConnection.CurrentValue.InvokeAsync(methodName, input, cancellationToken).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    _logger.LogInformation("{0} Completed to invoke method {1}", _guid, methodName);
                    return;
                }

                _logger.LogError("{0} Failed to invoke method {1}: {2}", _guid, methodName, task.Exception?.Message);
            });
        }

        public async Task<TResult> InvokeAsync<TInput, TResult>(string methodName, TInput input, CancellationToken cancellationToken = default)
        {
            if (_hubConnection.CurrentValue?.State != HubConnectionState.Connected && _continueToReconnect.CurrentValue)
            {
                _logger.LogDebug("{0} Connection is not established. Attempting to connect...", _guid);

                // Attempt to connect if the connection is not established
                await Connect(cancellationToken);

                if (_hubConnection.CurrentValue?.State != HubConnectionState.Connected)
                {
                    _logger.LogError("{0} Can't establish connection with Remote.", _guid);
                    return default!;
                }
            }
            if (_hubConnection.CurrentValue == null)
            {
                _logger.LogError("{0} HubConnection is null. Can't invoke method {1}.", _guid, methodName);
                return default!;
            }

            return await _hubConnection.CurrentValue.InvokeAsync<TResult>(methodName, input, cancellationToken).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    _logger.LogInformation("{0} Completed to invoke method {1}", _guid, methodName);
                    return task.Result;
                }

                _logger.LogError("{0} Failed to invoke method {1}: {2}", _guid, methodName, task.Exception?.Message);
                return default!;
            });
        }

        public async Task<bool> Connect(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("{0} Connect.", _guid);

            if (_hubConnection.Value?.State == HubConnectionState.Connected)
            {
                _logger.LogDebug("{0} Already connected. Ignoring.", _guid);
                return true;
            }

            _continueToReconnect.Value = false;

            // Dispose the previous internal CancellationTokenSource if it exists
            CancelInternalToken(dispose: true);

            if (_hubConnection.Value != null)
                await _hubConnection.Value.StopAsync();

            _continueToReconnect.Value = true;

            internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (connectionTask != null)
            {
                _logger.LogDebug("{0} Connection task already exists. Waiting for the completion... {1}.", _guid, Endpoint);
                // Create a new task that waits for the existing task but can be canceled independently
                return await Task.Run(async () =>
                {
                    try
                    {
                        await connectionTask; // Wait for the existing connection task
                        return _hubConnection.Value?.State == HubConnectionState.Connected;
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("{0} Connection task was canceled {1}.", _guid, Endpoint);
                        return false;
                    }
                }, internalCts.Token);
            }

            try
            {
                connectionTask = InternalConnect(internalCts.Token);
                return await connectionTask;
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} Error during connection: {1}\n{2}", _guid, ex.Message, ex.StackTrace);
                return false;
            }
            finally
            {
                connectionTask = null;
            }
        }

        void CancelInternalToken(bool dispose = false)
        {
            if (internalCts != null)
            {
                if (!internalCts.IsCancellationRequested)
                    internalCts.Cancel();

                if (dispose)
                {
                    internalCts.Dispose();
                    internalCts = null;
                }
            }
        }

        async Task<bool> InternalConnect(CancellationToken cancellationToken)
        {
            _logger.LogTrace("{0} InternalConnect", _guid);

            if (_hubConnection.Value == null)
            {
                hubConnectionLogger?.Dispose();
                hubConnectionObservable?.Dispose();

                _logger.LogDebug("{0} Creating new HubConnection instance {1}.", _guid, Endpoint);
                var hubConnection = await _hubConnectionBuilder.CreateConnectionAsync(Endpoint);
                if (hubConnection == null)
                {
                    _logger.LogError("{0} Can't create connection instance. Something may be wrong with Connection Config {1}.", _guid, Endpoint);
                    return false;
                }

                _logger.LogDebug("{0} Created new HubConnection instance {1}.", _guid, Endpoint);

                _hubConnection.Value = hubConnection;

                hubConnectionLogger = new(_logger, hubConnection, guid: _guid);

                hubConnectionObservable = new(hubConnection);
                hubConnectionObservable.Closed
                    .Subscribe(_ => connectionTask = null)
                    .RegisterTo(cancellationToken);
                hubConnectionObservable.Closed
                    .Where(_ => _continueToReconnect.CurrentValue)
                    .Where(_ => !cancellationToken.IsCancellationRequested)
                    .Subscribe(async _ =>
                    {
                        _logger.LogWarning("{0} Connection closed. Attempting to reconnect... {1}.", _guid, Endpoint);
                        await InternalConnect(cancellationToken);
                    })
                    .RegisterTo(cancellationToken);
            }

            await Task.Delay(50, cancellationToken);

            _logger.LogDebug("{0} Connecting to {1}...", _guid, Endpoint);
            while (_continueToReconnect.CurrentValue && !cancellationToken.IsCancellationRequested)
            {
                var connection = _hubConnection.CurrentValue;
                if (connection == null)
                {
                    _logger.LogTrace("{0} Waiting before retry... {1}", _guid, Endpoint);
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken); // Wait before retrying
                    continue;
                }

                _logger.LogInformation("{0} Starting connection to {1}...", _guid, Endpoint);
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var task = connection.StartAsync(cts.Token);
                try
                {
                    await Task.WhenAny(task.WaitAsync(TimeSpan.FromSeconds(30), TimeProvider.System, cancellationToken), Task.Delay(TimeSpan.FromSeconds(3), cancellationToken));
                    if (!task.IsCompletedSuccessfully)
                    {
                        if (_continueToReconnect.CurrentValue && !cancellationToken.IsCancellationRequested)
                        {
                            _logger.LogTrace("{0} Waiting before retry... {1}", _guid, Endpoint);
                            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken); // Wait before retrying
                        }
                        continue;
                    }
                    _logger.LogInformation("{0} Connection started successfully {1}.", _guid, Endpoint);
                    _connectionState.Value = HubConnectionState.Connected;
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("{0} Failed to start connection. {1} - {2}\n{3}", _guid, Endpoint, ex.Message, ex.StackTrace);
                }
                finally
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                if (_continueToReconnect.CurrentValue && !cancellationToken.IsCancellationRequested)
                {
                    _logger.LogTrace("{0} Waiting before retry... {1}", _guid, Endpoint);
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken); // Wait before retrying
                }
            }
            return false;
        }

        public Task Disconnect(CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("{0} Disconnect.", _guid);
            connectionTask = null;
            _continueToReconnect.Value = false;

            // Cancel the internal token to stop any ongoing connection attempts
            CancelInternalToken(dispose: false);

            if (_hubConnection.Value == null)
                return Task.CompletedTask;

            return _hubConnection.Value.StopAsync(cancellationToken).ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    _logger.LogInformation("{0} HubConnection stopped successfully.", _guid);
                }
                else if (task.Exception != null)
                {
                    _logger.LogError("{0} Error while stopping HubConnection: {1}\n{2}", _guid, task.Exception.Message, task.Exception.StackTrace);
                }
                _connectionState.Value = HubConnectionState.Disconnected;
            });
        }

        public void Dispose()
        {
#pragma warning disable CS4014
            DisposeAsync();
            // DisposeAsync().Wait();
            // Unity won't reload Domain if we call DisposeAsync().Wait() here.
#pragma warning restore CS4014
        }

        public async Task DisposeAsync()
        {
            _logger.LogTrace("{0} DisposeAsync.", _guid);

            if (!_continueToReconnect.IsDisposed)
                _continueToReconnect.Value = false;

            _disposables.Dispose();
            connectionTask = null;

            hubConnectionLogger?.Dispose();
            hubConnectionObservable?.Dispose();

            _connectionState.Dispose();
            _continueToReconnect.Dispose();

            CancelInternalToken(dispose: true);

            if (_hubConnection.CurrentValue != null)
            {
                try
                {
                    var tempHubConnection = _hubConnection.Value;

                    _hubConnection.Value = null;
                    _hubConnection.Dispose();

                    if (tempHubConnection != null)
                    {
                        await tempHubConnection.StopAsync()
                            .ContinueWith(task =>
                            {
                                try
                                {
                                    tempHubConnection.DisposeAsync();
                                }
                                catch { }
                            });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error during async disposal: {0}\n{1}", ex.Message, ex.StackTrace);
                }
            }

            _hubConnection.Dispose();
        }

        ~ConnectionManager() => Dispose();
    }
}
