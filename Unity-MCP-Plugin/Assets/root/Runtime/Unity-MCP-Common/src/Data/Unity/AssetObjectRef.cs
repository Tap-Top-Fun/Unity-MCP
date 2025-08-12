#nullable enable
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.ReflectorNet.Model.Unity
{
    [System.Serializable]
    [Description("Reference to UnityEngine.Object asset instance. It could be Material, ScriptableObject, Prefab, and any other Asset. Anything located in the Assets folder.")]
    public class AssetObjectRef : ObjectRef
    {
        [JsonInclude, JsonPropertyName("assetPath")]
        [Description("Path to the asset within the project. Starts with 'Assets/'")]
        public string? assetPath;

        [JsonInclude, JsonPropertyName("assetGuid")]
        [Description("Unique identifier for the asset.")]
        public string? assetGuid;

        public AssetObjectRef() : this(id: 0) { }
        public AssetObjectRef(int id) => instanceID = id;
        public AssetObjectRef(string assetPath) => this.assetPath = assetPath;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            if (instanceID != 0)
                stringBuilder.Append($"instanceID={instanceID}");

            if (!StringUtils.IsNullOrEmpty(assetPath))
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append($"assetPath={assetPath}");
            }

            if (!StringUtils.IsNullOrEmpty(assetGuid))
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(", ");
                stringBuilder.Append($"assetGuid={assetGuid}");
            }
            if (stringBuilder.Length == 0)
                return $"instanceID={instanceID}";

            return stringBuilder.ToString();
        }
    }
}