using System;
using System.Reflection;

namespace com.IvanMurzak.Unity.MCP.Common
{
    public class ToolMethodData
    {
        public string Name => Attribute.Name;
        public Type ClassType { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public McpPluginToolAttribute Attribute { get; set; }

        public ToolMethodData(Type classType, MethodInfo methodInfo, McpPluginToolAttribute attribute)
        {
            ClassType = classType;
            MethodInfo = methodInfo;
            Attribute = attribute;
        }
    }
}