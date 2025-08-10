using System;

namespace com.IvanMurzak.Unity.MCP.Common
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class McpPluginToolAttribute : Attribute
    {
        public string Name { get; set; }
        public string? Title { get; set; }

        public McpPluginToolAttribute(string name, string? title = null)
        {
            Name = name;
            Title = title;
        }
    }
}