#if !UNITY_5_3_OR_NEWER
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading.Tasks;

namespace com.IvanMurzak.Unity.MCP.Server.API
{
    public partial class Tool_Scene
    {
        [McpServerTool
        (
            Name = "Reflection_CommandRun",
            Title = "Execute C# code using Roslyn"
        )]
        [Description(@"Execute the C# code using Roslyn for compile and run it immediately.
This code won't be persistent.")]
        public ValueTask<CallToolResponse> CommandRun
        (
            [Description("C# code to execute. It should be a valid C# code that can be compiled and run immediately.")]
            string csharpCode
        )
        {
            return ToolRouter.Call("Reflection_CommandRun", arguments =>
            {
                arguments[nameof(csharpCode)] = csharpCode;
            });
        }
    }
}
#endif