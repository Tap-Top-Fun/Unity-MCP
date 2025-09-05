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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.API.TestRunner;
using com.IvanMurzak.Unity.MCP.Runtime.DomainReload;
using com.IvanMurzak.Unity.MCP.Utils;
using com.IvanMurzak.Unity.MCP.Common.Model;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public static partial class Tool_TestRunner
    {
        private static bool _isTestRunning;
        private static readonly object _testLock = new();

        [McpPluginTool
        (
            "TestRunner_Run",
            Title = "Run Unity Tests"
        )]
        [Description(@"Execute Unity tests and return detailed results. Supports filtering by test mode, assembly, namespace, class, and method.
Be default recommended to use 'EditMode' for faster iteration during development.")]
        public static ResponseCallTool Run
        (
            [Description("Test mode to run. Options: 'EditMode', 'PlayMode', 'All'. Default: 'EditMode'")]
            string testMode = "EditMode",
            [Description("Specific test assembly name to run (optional). Example: 'Assembly-CSharp-Editor-testable'")]
            string? testAssembly = null,
            [Description("Specific test namespace to run (optional). Example: 'MyTestNamespace'")]
            string? testNamespace = null,
            [Description("Specific test class name to run (optional). Example: 'MyTestClass'")]
            string? testClass = null,
            [Description("Specific fully qualified test method to run (optional). Example: 'MyTestNamespace.FixtureName.TestName'")]
            string? testMethod = null,
            [Description("Original MCP request (internal use - automatically provided by framework)")]
            RequestCallTool? _originalRequest = null
        )
        {
            try
            {
                // Generate unique operation ID for this test run
                var operationId = Guid.NewGuid().ToString();

                // Validate test mode
                if (!IsValidTestMode(testMode))
                    return ResponseCallTool.Error(Error.InvalidTestMode(testMode));

                // Get timeout from MCP server configuration
                var timeoutMs = McpPluginUnity.TimeoutMs;
                if (McpPluginUnity.IsLogActive(LogLevel.Debug))
                    Debug.Log($"[TestRunner] Using timeout: {timeoutMs} ms (from MCP plugin configuration)");

                // Get Test Runner API (must be on main thread)
                if (_testRunnerApi == null)
                    return ResponseCallTool.Error(Error.TestRunnerNotAvailable());

                var postTask = MainThread.Instance.RunAsync(async () =>
                {
                    try
                    {
                        // Check if tests are already running
                        lock (_testLock)
                        {
                            if (_isTestRunning)
                            {
                                McpPlugin.Instance!.RpcRouter!.NotifyToolRequestCompleted(
                                    ResponseCallTool.Error("Test execution is already in progress. Please wait for the current test run to complete."));
                                return;
                            }

                            _isTestRunning = true;
                        }
                        if (testMode == "All")
                        {
                            // Create filter parameters
                            var filterParams = new TestFilterParameters(testAssembly, testNamespace, testClass, testMethod);

                            // Check which modes have matching tests
                            var editModeTestCount = await GetMatchingTestCount(_testRunnerApi, TestMode.EditMode, filterParams);
                            var playModeTestCount = await GetMatchingTestCount(_testRunnerApi, TestMode.PlayMode, filterParams);

                            // If neither mode has tests, return error
                            if (editModeTestCount == 0 && playModeTestCount == 0)
                            {
                                await McpPlugin.Instance!.RpcRouter!.NotifyToolRequestCompleted(
                                    ResponseCallTool.Error(Error.NoTestsFound(filterParams)));
                                return;
                            }

                            // Handle "All" mode by running only the modes that have matching tests
                            var modesToRun = new List<string>();
                            if (editModeTestCount > 0) modesToRun.Add("EditMode");
                            if (playModeTestCount > 0) modesToRun.Add("PlayMode");

                            if (McpPluginUnity.IsLogActive(LogLevel.Info))
                                Debug.Log($"[TestRunner] Running tests in modes: {string.Join(", ", modesToRun)} (EditMode: {editModeTestCount}, PlayMode: {playModeTestCount})");
                            var response = await RunSequentialTests(_testRunnerApi, filterParams, timeoutMs, editModeTestCount > 0, playModeTestCount > 0, operationId, _originalRequest);
                            await McpPlugin.Instance!.RpcRouter!.NotifyToolRequestCompleted(ResponseCallTool.Success(response));
                        }
                        else
                        {
                            // Create filter parameters
                            var filterParams = new TestFilterParameters(testAssembly, testNamespace, testClass, testMethod);

                            // Convert string to TestMode enum
                            var testModeEnum = testMode == "EditMode"
                                ? TestMode.EditMode
                                : TestMode.PlayMode;

                            if (McpPluginUnity.IsLogActive(LogLevel.Info))
                                Debug.Log($"[TestRunner] Running {testMode} tests with filters: {filterParams}");
                            // Validate specific test mode filter
                            var validation = await ValidateTestFilters(_testRunnerApi, testModeEnum, filterParams);
                            if (validation != null)
                            {
                                await McpPlugin.Instance!.RpcRouter!.NotifyToolRequestCompleted(ResponseCallTool.Success(validation));
                                return;
                            }

                            Debug.Log($"[TestRunner] ------------------------------------- Running {testMode} tests.");

                            var resultCollector = await RunSingleTestModeWithCollector(testModeEnum, _testRunnerApi, filterParams, timeoutMs, operationId, _originalRequest);

                            Debug.Log($"[TestRunner] ------------------------------------- Completed {testMode} tests.");
                            // For PlayMode tests, domain reload may have occurred - results will be sent async
                            if (testModeEnum == TestMode.PlayMode && HasDomainReloadPersistence(operationId))
                            {
                                await McpPlugin.Instance!.RpcRouter!.NotifyToolRequestCompleted(
                                    ResponseCallTool.Success("[Info] PlayMode test execution started. Results will be sent asynchronously after domain reload completion."));
                                return;
                            }

                            await McpPlugin.Instance!.RpcRouter!.NotifyToolRequestCompleted(
                                ResponseCallTool.Success(resultCollector.FormatTestResults()));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.LogError($"[TestRunner] ------------------------------------- Canceled {testMode} tests.");
                        await McpPlugin.Instance!.RpcRouter!.NotifyToolRequestCompleted(
                            ResponseCallTool.Error(Error.TestTimeout(McpPluginUnity.TimeoutMs)));
                        return;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        Debug.LogError($"[TestRunner] ------------------------------------- Exception {testMode} tests.");
                        await McpPlugin.Instance!.RpcRouter!.NotifyToolRequestCompleted(
                            ResponseCallTool.Error(Error.TestExecutionFailed(ex.Message)));
                        return;
                    }
                    finally
                    {
                        // Always release the lock when done
                        lock (_testLock)
                        {
                            _isTestRunning = false;
                        }
                    }
                });

                return ResponseCallTool.Processing();
            }
            catch (OperationCanceledException)
            {
                return ResponseCallTool.Error(Error.TestTimeout(McpPluginUnity.TimeoutMs));
            }
            catch (Exception ex)
            {
                return ResponseCallTool.Error(Error.TestExecutionFailed(ex.Message));
            }
        }

        private static bool IsValidTestMode(string testMode) => testMode is "EditMode" or "PlayMode" or "All";

        private static Filter CreateTestFilter(TestMode testMode, TestFilterParameters filterParams)
        {
            var filter = new Filter
            {
                testMode = testMode
            };

            if (!string.IsNullOrEmpty(filterParams.TestAssembly))
                filter.assemblyNames = new[] { filterParams.TestAssembly };

            var groupNames = new List<string>();
            var testNames = new List<string>();

            // Handle specific test method in FixtureName.TestName format
            if (!string.IsNullOrEmpty(filterParams.TestMethod))
                testNames.Add(filterParams.TestMethod!);

            // Handle namespace filtering with regex (shared pattern ensures validation sync)
            if (!string.IsNullOrEmpty(filterParams.TestNamespace))
                groupNames.Add(CreateNamespaceRegexPattern(filterParams.TestNamespace!));

            // Handle class filtering with regex (shared pattern ensures validation sync)
            if (!string.IsNullOrEmpty(filterParams.TestClass))
                groupNames.Add(CreateClassRegexPattern(filterParams.TestClass!));

            if (groupNames.Any())
                filter.groupNames = groupNames.ToArray();

            if (testNames.Any())
                filter.testNames = testNames.ToArray();

            return filter;
        }

        /// <summary>
        /// Creates a regex pattern for namespace filtering that matches Unity's Filter.groupNames behavior.
        /// This ensures our validation logic (CountFilteredTests) matches exactly what Unity's TestRunner will execute.
        /// Pattern: "^{namespace}\." - matches tests in the specified namespace and its subnamespaces.
        /// </summary>
        /// <param name="namespaceName">The namespace to filter by</param>
        /// <returns>Regex pattern for Unity's Filter.groupNames field</returns>
        private static string CreateNamespaceRegexPattern(string namespaceName)
            => $"^{EscapeRegex(namespaceName)}\\.";

        /// <summary>
        /// Creates a regex pattern for class filtering that matches Unity's Filter.groupNames behavior.
        /// This ensures our validation logic (CountFilteredTests) matches exactly what Unity's TestRunner will execute.
        /// Pattern: "^.*\.{className}\.[^\.]+$" - matches any test class with the specified name followed by a method name.
        /// </summary>
        /// <param name="className">The class name to filter by</param>
        /// <returns>Regex pattern for Unity's Filter.groupNames field</returns>
        static string CreateClassRegexPattern(string className)
            => $"^.*\\.{EscapeRegex(className)}\\.[^\\.]+$";

        /// <summary>
        /// Escapes special regex characters to ensure literal string matching.
        /// Used by the shared regex pattern builders to safely handle user input that may contain regex metacharacters.
        /// </summary>
        /// <param name="input">The string to escape</param>
        /// <returns>Regex-safe escaped string</returns>
        static string EscapeRegex(string input)
            => Regex.Escape(input);

        static async Task<int> GetMatchingTestCount(TestRunnerApi testRunnerApi, TestMode testMode, TestFilterParameters filterParams)
        {
            try
            {
                var tcs = new TaskCompletionSource<int>();

                // Retrieve test list without running tests
                await MainThread.Instance.RunAsync(() =>
                {
                    testRunnerApi.RetrieveTestList(testMode, (testRoot) =>
                    {
                        var testCount = testRoot != null
                            ? CountFilteredTests(testRoot, filterParams)
                            : 0;

                        if (McpPluginUnity.IsLogActive(LogLevel.Info))
                            Debug.Log($"[TestRunner] {testCount} {testMode} tests matched for {filterParams}");

                        tcs.SetResult(testCount);
                    });
                });

                // Wait for the test count result with timeout
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                    throw new OperationCanceledException("Test list retrieval timed out");

                return await tcs.Task;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        static async Task<string?> ValidateTestFilters(TestRunnerApi testRunnerApi, TestMode testMode, TestFilterParameters filterParams)
        {
            try
            {
                var testCount = await GetMatchingTestCount(testRunnerApi, testMode, filterParams);
                if (testCount == 0)
                    return Error.NoTestsFound(filterParams);

                return null; // No error, tests found
            }
            catch (Exception ex)
            {
                return Error.TestExecutionFailed($"Filter validation failed: {ex.Message}");
            }
        }

        static int CountFilteredTests(ITestAdaptor test, TestFilterParameters filterParams)
        {
            // If no filters are specified, count all tests
            if (!filterParams.HasAnyFilter)
                return TestResultCollector.CountTests(test);

            var count = 0;

            // Check if this test matches the filters
            if (!test.IsSuite)
            {
                var matches = false;

                // Check assembly filter using UniqueName which contains assembly information
                if (!string.IsNullOrEmpty(filterParams.TestAssembly))
                {
                    var dllIndex = test.UniqueName.IndexOf(".dll");
                    if (dllIndex > 0)
                    {
                        var assemblyName = test.UniqueName[..dllIndex];
                        if (assemblyName.Equals(filterParams.TestAssembly, StringComparison.OrdinalIgnoreCase))
                            matches = true;
                    }
                }

                // Check namespace filter using same regex pattern as Filter.groupNames (ensures sync with Unity's execution)
                if (!matches && !string.IsNullOrEmpty(filterParams.TestNamespace))
                {
                    var namespacePattern = CreateNamespaceRegexPattern(filterParams.TestNamespace!);
                    if (Regex.IsMatch(test.FullName, namespacePattern))
                        matches = true;
                }

                // Check class filter using same regex pattern as Filter.groupNames (ensures sync with Unity's execution)
                if (!matches && !string.IsNullOrEmpty(filterParams.TestClass))
                {
                    var classPattern = CreateClassRegexPattern(filterParams.TestClass!);
                    if (Regex.IsMatch(test.FullName, classPattern))
                        matches = true;
                }

                // Check method filter (FixtureName.TestName format, same as Filter.testNames)
                if (!matches && !string.IsNullOrEmpty(filterParams.TestMethod))
                {
                    if (test.FullName.Equals(filterParams.TestMethod, StringComparison.OrdinalIgnoreCase))
                        matches = true;
                }

                if (matches)
                    count = 1;
            }

            // Recursively check children
            if (test.HasChildren)
            {
                foreach (var child in test.Children)
                    count += CountFilteredTests(child, filterParams);
            }

            return count;
        }

        static async Task<string> RunSequentialTests(
            TestRunnerApi testRunnerApi,
            TestFilterParameters filterParams,
            int timeoutMs,
            bool runEditMode,
            bool runPlayMode,
            string operationId,
            RequestCallTool? originalRequest)
        {
            var combinedCollector = new CombinedTestResultCollector();
            var totalStartTime = DateTime.Now;

            try
            {
                var remainingTimeoutMs = timeoutMs;

                // Run EditMode tests if they exist
                if (runEditMode)
                {
                    if (McpPluginUnity.IsLogActive(LogLevel.Info))
                        Debug.Log($"[TestRunner] Starting EditMode tests...");

                    var editModeStartTime = DateTime.Now;
                    var editModeCollector = await RunSingleTestModeWithCollector(TestMode.EditMode, testRunnerApi, filterParams, timeoutMs, $"{operationId}_edit", originalRequest);

                    combinedCollector.AddResults(editModeCollector);

                    var editModeDuration = DateTime.Now - editModeStartTime;
                    remainingTimeoutMs = Math.Max(1000, timeoutMs - (int)editModeDuration.TotalMilliseconds);

                    if (McpPluginUnity.IsLogActive(LogLevel.Info))
                        Debug.Log($"[TestRunner] EditMode tests completed in {editModeDuration:mm\\:ss\\.fff}.");
                }
                else
                {
                    if (McpPluginUnity.IsLogActive(LogLevel.Info))
                        Debug.Log($"[TestRunner] Skipping EditMode tests (no matching tests found).");
                }

                // Run PlayMode tests if they exist
                if (runPlayMode)
                {
                    if (McpPluginUnity.IsLogActive(LogLevel.Info))
                        Debug.Log($"[TestRunner] Starting PlayMode tests with {remainingTimeoutMs}ms timeout...");

                    var playModeCollector = await RunSingleTestModeWithCollector(TestMode.PlayMode, testRunnerApi, filterParams, remainingTimeoutMs, $"{operationId}_play", originalRequest);

                    // Check if PlayMode triggered domain reload
                    if (HasDomainReloadPersistence($"{operationId}_play"))
                    {
                        return "[Info] PlayMode test execution started. Results will be sent asynchronously after domain reload completion.";
                    }

                    combinedCollector.AddResults(playModeCollector);

                    if (McpPluginUnity.IsLogActive(LogLevel.Info))
                        Debug.Log($"[TestRunner] PlayMode tests completed.");
                }
                else
                {
                    if (McpPluginUnity.IsLogActive(LogLevel.Info))
                        Debug.Log($"[TestRunner] Skipping PlayMode tests (no matching tests found).");
                }

                // Calculate total duration
                var totalDuration = DateTime.Now - totalStartTime;
                combinedCollector.SetTotalDuration(totalDuration);

                // Format combined results - handle case where only one mode ran
                if (runEditMode && runPlayMode)
                {
                    return combinedCollector.FormatCombinedResults();
                }
                else
                {
                    // Only one mode ran, use single mode formatting
                    var collectors = combinedCollector.GetAllCollectors();
                    if (collectors.Any())
                        return collectors.First().FormatTestResults();

                    return "[Success] No tests were executed (no matching tests found).";
                }
            }
            catch (Exception ex)
            {
                return Error.TestExecutionFailed($"Sequential test execution failed: {ex.Message}");
            }
        }

        static async Task<TestResultCollector> RunSingleTestModeWithCollector(
            TestMode testMode,
            TestRunnerApi testRunnerApi,
            TestFilterParameters filterParams,
            int timeoutMs,
            string? operationId = null,
            RequestCallTool? originalRequest = null)
        {
            var filter = CreateTestFilter(testMode, filterParams);
            var runNumber = testMode == TestMode.EditMode
                ? 1
                : 2;
            var resultCollector = new TestResultCollector(testMode);

            // Save state for domain reload continuation if this is PlayMode
            if (testMode == TestMode.PlayMode && !string.IsNullOrEmpty(operationId) && originalRequest != null)
            {
                TestRunnerDomainReloadHandler.SaveTestExecutionState(
                    operationId!,
                    testMode,
                    filter,
                    timeoutMs,
                    runNumber,
                    DateTime.Now,
                    originalRequest
                );
            }

            testRunnerApi.RegisterCallbacks(resultCollector);
            var executionSettings = new ExecutionSettings(filter);
            var output = testRunnerApi.Execute(executionSettings);

            Debug.Log($"[TestRunner] ------------------------------------- Started Execute for {testMode} tests. output={output}");

            try
            {
                Debug.Log($"[TestRunner] ------------------------------------- Started WaitForCompletionAsync.");
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
                await resultCollector.WaitForCompletionAsync(timeoutCts.Token);

                Debug.Log($"[TestRunner] ------------------------------------- Completed WaitForCompletionAsync.");

                // Clean up domain reload state on successful completion
                if (testMode == TestMode.PlayMode && !string.IsNullOrEmpty(operationId))
                {
                    DomainReloadManager.CompleteOperation("TestRunner", operationId!);
                }

                return resultCollector;
            }
            catch (OperationCanceledException)
            {
                if (McpPluginUnity.IsLogActive(LogLevel.Warning))
                    Debug.LogWarning($"[TestRunner] {testMode} tests timed out after {timeoutMs} ms.");

                // Clean up domain reload state on timeout
                if (testMode == TestMode.PlayMode && !string.IsNullOrEmpty(operationId))
                {
                    DomainReloadManager.CompleteOperation("TestRunner", operationId!);
                }

                return resultCollector;
            }
            finally
            {
                testRunnerApi.UnregisterCallbacks(resultCollector);
            }
        }

        /// <summary>
        /// Checks if domain reload persistence was triggered for this operation.
        /// </summary>
        /// <param name="operationId">Operation ID to check for</param>
        /// <returns>True if domain reload persistence exists</returns>
        static bool HasDomainReloadPersistence(string operationId)
        {
            var key = $"TestRunner_{operationId}";
            return DomainReloadPersistence.HasData(key);
        }
    }
}
