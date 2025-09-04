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
using System.Threading.Tasks;
using com.IvanMurzak.Unity.MCP.Common.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.Unity.MCP.Runtime.DomainReload;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API.TestRunner
{
    /// <summary>
    /// Handles collection of TestRunner results after domain reload and sends them back via SignalR.
    /// </summary>
    [InitializeOnLoad]
    public class TestRunnerDomainReloadHandler : IDomainReloadHandler
    {
        private const string OPERATION_TYPE = "TestRunner";

        static TestRunnerDomainReloadHandler()
        {
            // Register this handler with the domain reload manager
            DomainReloadManager.RegisterHandler(OPERATION_TYPE, new TestRunnerDomainReloadHandler());
        }

        public void ResumeAfterDomainReload(string operationId, object state)
        {
            if (state is TestRunnerDomainReloadState testRunnerState)
            {
                if (McpPluginUnity.IsLogActive(LogLevel.Info))
                    Debug.Log($"[TestRunnerDomainReloadHandler] Starting result collection after domain reload: {operationId}");

                // Start result collection on the main thread
                MainThread.Instance.Run(async () =>
                {
                    await CollectResultsAndSendResponse(operationId, testRunnerState);
                });
            }
            else
            {
                Debug.LogError($"[TestRunnerDomainReloadHandler] Invalid state type for operation {operationId}");
                DomainReloadManager.CompleteOperation(OPERATION_TYPE, operationId);
            }
        }

        private async Task CollectResultsAndSendResponse(string operationId, TestRunnerDomainReloadState state)
        {
            try
            {
                // Create new TestRunnerApi instance
                var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
                if (testRunnerApi == null)
                {
                    await SendErrorResponse(state.OriginalRequest, "TestRunnerApi not available after domain reload");
                    return;
                }

                // Calculate remaining timeout
                var elapsed = DateTime.Now - state.StartTime;
                var remainingTimeout = Math.Max(5000, state.TimeoutMs - (int)elapsed.TotalMilliseconds);

                if (McpPluginUnity.IsLogActive(LogLevel.Info))
                    Debug.Log($"[TestRunnerDomainReloadHandler] Collecting results with {remainingTimeout}ms timeout remaining");

                // Create result collector for PlayMode execution
                var resultCollector = new TestResultCollector(state.TestMode);

                // Register callbacks and execute
                testRunnerApi.RegisterCallbacks(resultCollector);
                var executionSettings = new ExecutionSettings(state.Filter);
                testRunnerApi.Execute(executionSettings);

                // Wait for completion
                var timeoutCts = new System.Threading.CancellationTokenSource(TimeSpan.FromMilliseconds(remainingTimeout));
                try
                {
                    await resultCollector.WaitForCompletionAsync(timeoutCts.Token);

                    // Format results and send response
                    var results = resultCollector.FormatTestResults();
                    await SendSuccessResponse(state.OriginalRequest, results);
                }
                catch (OperationCanceledException)
                {
                    if (McpPluginUnity.IsLogActive(LogLevel.Warning))
                        Debug.LogWarning($"[TestRunnerDomainReloadHandler] Test result collection timed out after domain reload");

                    var partialResults = resultCollector.FormatTestResults();
                    await SendSuccessResponse(state.OriginalRequest, $"[Timeout] Test result collection timed out after {remainingTimeout}ms\n\n{partialResults}");
                }
                finally
                {
                    testRunnerApi.UnregisterCallbacks(resultCollector);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"[TestRunnerDomainReloadHandler] Failed to collect test results: {ex.Message}");
                await SendErrorResponse(state.OriginalRequest, $"Failed to collect test results after domain reload: {ex.Message}");
            }
            finally
            {
                // Clean up the operation state
                DomainReloadManager.CompleteOperation(OPERATION_TYPE, operationId);
            }
        }

        private async Task SendSuccessResponse(RequestCallTool originalRequest, string result)
        {
            try
            {
                var mcpPlugin = McpPlugin.Instance;
                if (mcpPlugin?.RpcRouter == null)
                {
                    Debug.LogError("[TestRunnerDomainReloadHandler] RpcRouter instance not available for sending response");
                    return;
                }

                // Send response via SignalR
                if (McpPluginUnity.IsLogActive(LogLevel.Info))
                    Debug.Log($"[TestRunnerDomainReloadHandler] Sending successful test results for request: {originalRequest.RequestID}");

                // Create response using the original request ID
                await mcpPlugin.RpcRouter.NotifyToolRequestCompleted(
                    ResponseCallTool.Success(result).SetRequestID(originalRequest.RequestID)
                );

                if (McpPluginUnity.IsLogActive(LogLevel.Debug))
                    Debug.Log($"[TestRunnerDomainReloadHandler] Test results sent successfully: {result[..Math.Min(200, result.Length)]}...");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"[TestRunnerDomainReloadHandler] Failed to send success response: {ex.Message}");
            }
        }

        private async Task SendErrorResponse(RequestCallTool originalRequest, string error)
        {
            try
            {
                var mcpPlugin = McpPlugin.Instance;
                if (mcpPlugin?.RpcRouter == null)
                {
                    Debug.LogError("[TestRunnerDomainReloadHandler] RpcRouter instance not available for sending error response");
                    return;
                }

                // Send response via SignalR
                if (McpPluginUnity.IsLogActive(LogLevel.Info))
                    Debug.Log($"[TestRunnerDomainReloadHandler] Sending error response for request: {originalRequest.RequestID}");

                // Create response using the original request ID
                await mcpPlugin.RpcRouter.NotifyToolRequestCompleted(
                    ResponseCallTool.Error(error).SetRequestID(originalRequest.RequestID)
                );
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"[TestRunnerDomainReloadHandler] Failed to send error response: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the test execution state and original request before domain reload.
        /// </summary>
        public static void SaveTestExecutionState(string operationId, TestMode testMode, Filter filter, int timeoutMs, int runNumber, DateTime startTime, RequestCallTool originalRequest)
        {
            var state = new TestRunnerDomainReloadState
            {
                TestMode = testMode,
                Filter = filter,
                TimeoutMs = timeoutMs,
                RunNumber = runNumber,
                StartTime = startTime,
                OriginalRequest = originalRequest
            };

            DomainReloadManager.SaveOperationState(OPERATION_TYPE, operationId, state);
        }
    }

    /// <summary>
    /// State data for TestRunner operations that survive domain reload.
    /// </summary>
    [Serializable]
    public class TestRunnerDomainReloadState
    {
        public TestMode TestMode { get; set; }
        public Filter Filter { get; set; } = new Filter();
        public int TimeoutMs { get; set; }
        public int RunNumber { get; set; }
        public DateTime StartTime { get; set; }
        public RequestCallTool OriginalRequest { get; set; } = new RequestCallTool();
    }
}