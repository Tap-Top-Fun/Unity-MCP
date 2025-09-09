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
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.API.TestRunner;
using com.IvanMurzak.Unity.MCP.Utils;
using com.IvanMurzak.Unity.MCP.Common.Model;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public static partial class Tool_TestRunner
    {
        [McpPluginTool
        (
            "TestRunner_Run",
            Title = "Run Unity Tests"
        )]
        [Description(@"Execute Unity tests and return detailed results. Supports filtering by test mode, assembly, namespace, class, and method.
Be default recommended to use 'EditMode' for faster iteration during development.")]
        public static ResponseCallTool Run
        (
            [Description("Test mode to run. Options: '" + nameof(TestMode.EditMode) + "', '" + nameof(TestMode.PlayMode) + "'. Default: '" + nameof(TestMode.EditMode) + "'")]
            TestMode testMode = TestMode.EditMode,
            [Description("Specific test assembly name to run (optional). Example: 'Assembly-CSharp-Editor-testable'")]
            string? testAssembly = null,
            [Description("Specific test namespace to run (optional). Example: 'MyTestNamespace'")]
            string? testNamespace = null,
            [Description("Specific test class name to run (optional). Example: 'MyTestClass'")]
            string? testClass = null,
            [Description("Specific fully qualified test method to run (optional). Example: 'MyTestNamespace.FixtureName.TestName'")]
            string? testMethod = null,
            [RequestID]
            string? requestId = null
        )
        {
            if (requestId == null || string.IsNullOrWhiteSpace(requestId))
                return ResponseCallTool.Error("Original request with valid RequestID must be provided.");

            Debug.Log($"[TestRunner] ------------------------------------- Preparing to run {testMode} tests.");

            return MainThread.Instance.Run(() =>
            {
                try
                {
                    Debug.Log($"[TestRunner] ------------------------------------- Preparing to run {testMode} tests 2.");
                    // Get Test Runner API (must be on main thread)
                    // if (TestRunnerApi == null)
                    // if (_testRunnerApi == null)
                    //     return ResponseCallTool.Error("[Error] Unity Test Runner API is not available");

                    Debug.Log($"[TestRunner] ------------------------------------- Preparing to run {testMode} tests 3.");
                    TestResultCollector.TestCallRequestID.Value = requestId;
                    Debug.Log($"[TestRunner] ------------------------------------- Preparing to run {testMode} tests 4.");

                    // Create filter parameters
                    var filterParams = new TestFilterParameters(testAssembly, testNamespace, testClass, testMethod);

                    if (McpPluginUnity.IsLogActive(LogLevel.Info))
                        Debug.Log($"[TestRunner] Running {testMode} tests with filters: {filterParams}");

                    // Validate specific test mode filter
                    var validation = ValidateTestFilters(TestRunnerApi, testMode, filterParams).Result;
                    Debug.Log($"[TestRunner] ------------------------------------- Validation completed: {validation}.");
                    if (validation != null)
                        return ResponseCallTool.Error(validation).SetRequestID(requestId);

                    Debug.Log($"[TestRunner] ------------------------------------- Running {testMode} tests.");

                    var filter = CreateTestFilter(testMode, filterParams);
                    TestRunnerApi.Execute(new ExecutionSettings(filter));

                    return ResponseCallTool.Processing().SetRequestID(requestId);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogError($"[TestRunner] ------------------------------------- Exception {testMode} tests.");
                    return ResponseCallTool.Error(Error.TestExecutionFailed(ex.Message));
                }
            });
        }

        static Filter CreateTestFilter(TestMode testMode, TestFilterParameters filterParams)
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

                testRunnerApi.RetrieveTestList(testMode, (testRoot) =>
                {
                    var testCount = testRoot != null
                        ? CountFilteredTests(testRoot, filterParams)
                        : 0;

                    if (McpPluginUnity.IsLogActive(LogLevel.Info))
                        Debug.Log($"[TestRunner] {testCount} {testMode} tests matched for {filterParams}");

                    tcs.SetResult(testCount);
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
                    var dllIndex = test.UniqueName.ToLowerInvariant().IndexOf(".dll");
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
    }
}
