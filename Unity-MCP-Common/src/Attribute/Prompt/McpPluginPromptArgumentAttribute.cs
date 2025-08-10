using System;

namespace com.IvanMurzak.Unity.MCP.Common
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class McpPluginPromptArgumentAttribute : Attribute
    {
        public string? Name { get; set; }

        public McpPluginPromptArgumentAttribute() { }
    }
}