#nullable enable
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace com.IvanMurzak.ReflectorNet.Model.Unity
{
    [Description(@"Find GameObject in opened Prefab or in the active Scene.")]
    public class GameObjectRef : AssetObjectRef
    {
        [JsonInclude, JsonPropertyName("instanceID")]
        [Description("Instance ID of the UnityEngine.Object. If this is '0' and 'path', 'name', 'assetPath' and 'assetGuid' is not provided, empty or null, then it will be used as 'null'.")]
        public override int instanceID { get; set; } = 0;

        [JsonInclude, JsonPropertyName("path")]
        [Description("Path of a GameObject in the hierarchy Sample 'character/hand/finger/particle'. Priority: 2.")]
        public string? path { get; set; } = null;

        [JsonInclude, JsonPropertyName("name")]
        [Description("Name of a GameObject in hierarchy. Priority: 3.")]
        public string? name { get; set; } = null;

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                if (instanceID != 0)
                    return true;
                if (!string.IsNullOrEmpty(path))
                    return true;
                if (!string.IsNullOrEmpty(name))
                    return true;
                if (!string.IsNullOrEmpty(assetPath))
                    return true;
                if (!string.IsNullOrEmpty(assetGuid))
                    return true;
                return false;
            }
        }

        public GameObjectRef() { }
        public GameObjectRef(int instanceID)
        {
            this.instanceID = instanceID;
        }

        public override string ToString()
        {
            if (instanceID != 0)
                return $"GameObject instanceID='{instanceID}'";
            if (!string.IsNullOrEmpty(path))
                return $"GameObject path='{path}'";
            if (!string.IsNullOrEmpty(name))
                return $"GameObject name='{name}'";
            if (!string.IsNullOrEmpty(assetPath))
                return $"GameObject assetPath='{assetPath}'";
            if (!string.IsNullOrEmpty(assetGuid))
                return $"GameObject assetGuid='{assetGuid}'";
            return "GameObject unknown";
        }
    }
}