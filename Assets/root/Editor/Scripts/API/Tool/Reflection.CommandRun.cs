#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Utils;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Reflection
    {
        [McpPluginTool
        (
            "Reflection_CommandRun",
            Title = "Execute C# code using Roslyn"
        )]
        [Description(@"Execute the C# code using Roslyn for compile and run it immediately.
This code won't be persistent.")]
        public string CommandRun
        (
            [Description("C# code to execute. It should be a valid C# code that can be compiled and run immediately.")]
            string csharpCode
        )
        {
            try
            {
                // Create script options with common references
                var options = ScriptOptions.Default
                    .WithReferences(typeof(object).Assembly) // mscorlib
                    .WithReferences(typeof(System.Console).Assembly) // System.Console
                    .WithReferences(typeof(System.Linq.Enumerable).Assembly) // System.Linq
                    .WithImports("System", "System.Linq", "System.Collections.Generic");

                // Execute the script
                var result = CSharpScript.EvaluateAsync(csharpCode, options).Result;

                if (result != null)
                {
                    return $"[Success] Execution completed. Result: {result}";
                }
                else
                {
                    return "[Success] Execution completed. No return value.";
                }
            }
            catch (CompilationErrorException ex)
            {
                return $"[Error] Compilation failed:\n{string.Join("\n", ex.Diagnostics)}";
            }
            catch (Exception ex)
            {
                return $"[Error] Runtime error: {ex.Message}";
            }
        }
    }
}