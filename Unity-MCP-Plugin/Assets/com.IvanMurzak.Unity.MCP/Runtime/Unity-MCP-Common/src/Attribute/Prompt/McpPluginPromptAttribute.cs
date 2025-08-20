using System;

namespace com.IvanMurzak.Unity.MCP.Common
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class McpPluginPromptAttribute : Attribute
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public McpPluginPromptAttribute() { }
    }
}