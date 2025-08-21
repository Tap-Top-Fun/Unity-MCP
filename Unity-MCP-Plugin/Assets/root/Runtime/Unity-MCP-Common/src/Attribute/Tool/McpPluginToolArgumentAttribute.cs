using System;

namespace com.IvanMurzak.Unity.MCP.Common
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class McpPluginToolArgumentAttribute : Attribute
    {
        public string? Name { get; set; }

        public McpPluginToolArgumentAttribute() { }
    }
}