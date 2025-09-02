/*
┌──────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)             │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-MCP)    │
│  Copyright (c) 2025 Ivan Murzak                                  │
│  Licensed under the Apache License, Version 2.0.                 │
│  See the LICENSE file in the project root for more information.  │
└──────────────────────────────────────────────────────────────────┘
*/
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.IO;
using System.Text.Json;
using UnityEngine;
using com.IvanMurzak.Unity.MCP.Editor;
using com.IvanMurzak.Unity.MCP.Utils;

namespace com.IvanMurzak.Unity.MCP.Runtime.DomainReload
{
    /// <summary>
    /// Universal system for persisting data across Unity domain reloads.
    /// Uses JSON files in the Library/mcp-server folder to store execution state.
    /// </summary>
    public static class DomainReloadPersistence
    {
        private static readonly string PersistenceFolder = Path.Combine(Startup.Server.ExecutableFolderRootPath, "persistence");

        /// <summary>
        /// Saves data to a persistent file that survives domain reloads.
        /// </summary>
        /// <typeparam name="T">Type of data to save</typeparam>
        /// <param name="key">Unique identifier for the data</param>
        /// <param name="data">Data to persist</param>
        public static void SaveData<T>(string key, T data)
        {
            try
            {
                EnsurePersistenceFolderExists();
                var filePath = GetFilePath(key);
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(filePath, json);

                if (McpPluginUnity.IsLogActive(LogLevel.Debug))
                    Debug.Log($"[DomainReloadPersistence] Saved data for key: {key}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DomainReloadPersistence] Failed to save data for key '{key}': {ex.Message}");
            }
        }

        /// <summary>
        /// Loads data from a persistent file.
        /// </summary>
        /// <typeparam name="T">Type of data to load</typeparam>
        /// <param name="key">Unique identifier for the data</param>
        /// <returns>Loaded data or default value if not found</returns>
        public static T? LoadData<T>(string key)
        {
            try
            {
                var filePath = GetFilePath(key);
                if (!File.Exists(filePath))
                    return default;

                var json = File.ReadAllText(filePath);
                var data = JsonSerializer.Deserialize<T>(json);

                if (McpPluginUnity.IsLogActive(LogLevel.Debug))
                    Debug.Log($"[DomainReloadPersistence] Loaded data for key: {key}");

                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DomainReloadPersistence] Failed to load data for key '{key}': {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Checks if persistent data exists for the given key.
        /// </summary>
        /// <param name="key">Unique identifier for the data</param>
        /// <returns>True if data exists</returns>
        public static bool HasData(string key)
        {
            var filePath = GetFilePath(key);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Removes persistent data for the given key.
        /// </summary>
        /// <param name="key">Unique identifier for the data</param>
        public static void RemoveData(string key)
        {
            try
            {
                var filePath = GetFilePath(key);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);

                    if (McpPluginUnity.IsLogActive(LogLevel.Debug))
                        Debug.Log($"[DomainReloadPersistence] Removed data for key: {key}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DomainReloadPersistence] Failed to remove data for key '{key}': {ex.Message}");
            }
        }

        /// <summary>
        /// Clears all persistent data files.
        /// </summary>
        public static void ClearAll()
        {
            try
            {
                if (Directory.Exists(PersistenceFolder))
                {
                    var files = Directory.GetFiles(PersistenceFolder, "*.json");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }

                    if (McpPluginUnity.IsLogActive(LogLevel.Debug))
                        Debug.Log($"[DomainReloadPersistence] Cleared {files.Length} persistent data files");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DomainReloadPersistence] Failed to clear persistent data: {ex.Message}");
            }
        }

        private static string GetFilePath(string key)
        {
            var safeKey = key.Replace("/", "_").Replace("\\", "_").Replace(":", "_");
            return Path.Combine(PersistenceFolder, $"{safeKey}.json");
        }

        private static void EnsurePersistenceFolderExists()
        {
            if (!Directory.Exists(PersistenceFolder))
            {
                Directory.CreateDirectory(PersistenceFolder);
            }
        }
    }
}