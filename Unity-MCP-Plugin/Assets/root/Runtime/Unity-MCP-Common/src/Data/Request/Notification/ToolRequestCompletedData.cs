#nullable enable

namespace com.IvanMurzak.Unity.MCP.Common.Model
{
    public class ToolRequestCompletedData
    {
        public string RequestId { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;

        public override string ToString()
            => $"RequestId: {RequestId}, ResponseJson: {ResponseJson}";
    }
}