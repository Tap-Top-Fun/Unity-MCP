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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using com.IvanMurzak.Unity.MCP.Utils;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.Unity.MCP.Runtime.DomainReload
{
    /// <summary>
    /// Manages continuation of MCP tool execution across Unity domain reloads.
    /// Uses InitializeOnLoad to detect domain reloads and resume interrupted operations.
    /// </summary>
    [InitializeOnLoad]
    public static class DomainReloadManager
    {
        private static readonly Dictionary<string, IDomainReloadHandler> _handlers = new();

        static DomainReloadManager()
        {
            // This runs after every domain reload
            MainThread.Instance.Run(CheckForPendingOperations);
        }

        /// <summary>
        /// Registers a handler for a specific operation type that can be resumed after domain reload.
        /// </summary>
        /// <param name="operationType">Unique identifier for the operation type</param>
        /// <param name="handler">Handler that can resume the operation</param>
        public static void RegisterHandler(string operationType, IDomainReloadHandler handler)
        {
            _handlers[operationType] = handler;

            if (McpPluginUnity.IsLogActive(LogLevel.Debug))
                Debug.Log($"[DomainReloadManager] Registered handler for operation type: {operationType}");
        }

        /// <summary>
        /// Unregisters a handler for a specific operation type.
        /// </summary>
        /// <param name="operationType">Unique identifier for the operation type</param>
        public static void UnregisterHandler(string operationType)
        {
            if (_handlers.Remove(operationType))
            {
                if (McpPluginUnity.IsLogActive(LogLevel.Debug))
                    Debug.Log($"[DomainReloadManager] Unregistered handler for operation type: {operationType}");
            }
        }

        /// <summary>
        /// Saves operation state that should be resumed after domain reload.
        /// </summary>
        /// <param name="operationType">Type of operation</param>
        /// <param name="operationId">Unique ID for this specific operation instance</param>
        /// <param name="state">State data to persist</param>
        public static void SaveOperationState(string operationType, string operationId, object state)
        {
            var key = $"{operationType}_{operationId}";
            var operationData = new DomainReloadOperationData
            {
                OperationType = operationType,
                OperationId = operationId,
                State = state,
                SavedAt = DateTime.Now
            };

            DomainReloadPersistence.SaveData(key, operationData);

            if (McpPluginUnity.IsLogActive(LogLevel.Info))
                Debug.Log($"[DomainReloadManager] Saved operation state: {operationType} ({operationId})");
        }

        /// <summary>
        /// Removes operation state after successful completion.
        /// </summary>
        /// <param name="operationType">Type of operation</param>
        /// <param name="operationId">Unique ID for this specific operation instance</param>
        public static void CompleteOperation(string operationType, string operationId)
        {
            var key = $"{operationType}_{operationId}";
            DomainReloadPersistence.RemoveData(key);

            if (McpPluginUnity.IsLogActive(LogLevel.Info))
                Debug.Log($"[DomainReloadManager] Completed operation: {operationType} ({operationId})");
        }

        private static void CheckForPendingOperations()
        {
            try
            {
                // Get all persistent data files
                var persistenceFolder = System.IO.Path.Combine(Editor.Startup.Server.ExecutableFolderRootPath, "persistence");
                if (!System.IO.Directory.Exists(persistenceFolder))
                    return;

                var files = System.IO.Directory.GetFiles(persistenceFolder, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                        var operationData = DomainReloadPersistence.LoadData<DomainReloadOperationData>(fileName);

                        if (operationData != null && _handlers.TryGetValue(operationData.OperationType, out var handler))
                        {
                            if (McpPluginUnity.IsLogActive(LogLevel.Info))
                                Debug.Log($"[DomainReloadManager] Resuming operation: {operationData.OperationType} ({operationData.OperationId})");

                            // Resume the operation on the main thread
                            MainThread.Instance.Run(() =>
                            {
                                handler.ResumeAfterDomainReload(operationData.OperationId, operationData.State);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[DomainReloadManager] Failed to process pending operation file '{file}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DomainReloadManager] Failed to check for pending operations: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Interface for handlers that can resume operations after domain reload.
    /// </summary>
    public interface IDomainReloadHandler
    {
        /// <summary>
        /// Called when an operation needs to be resumed after domain reload.
        /// </summary>
        /// <param name="operationId">Unique ID of the operation to resume</param>
        /// <param name="state">Saved state data</param>
        void ResumeAfterDomainReload(string operationId, object state);
    }

    /// <summary>
    /// Data structure for persisting operation state across domain reloads.
    /// </summary>
    [Serializable]
    public class DomainReloadOperationData
    {
        public string OperationType { get; set; } = string.Empty;
        public string OperationId { get; set; } = string.Empty;
        public object State { get; set; } = new object();
        public DateTime SavedAt { get; set; }
    }
}