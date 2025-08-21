using System;

namespace com.IvanMurzak.Unity.MCP.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class McpPluginPromptTypeAttribute : Attribute
    {
        public string? Path { get; set; }

        public McpPluginPromptTypeAttribute() { }
    }
}