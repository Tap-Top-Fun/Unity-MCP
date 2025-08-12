using System.ComponentModel;
using System.Text.Json.Serialization;

namespace com.IvanMurzak.ReflectorNet.Model.Unity
{
    [Description(@"Find GameObject in opened Prefab or in the active Scene.")]
    public class GameObjectRef
    {
        [JsonInclude, JsonPropertyName("instanceID")]
        [Description("GameObject 'instanceID'. Priority: 1. (Recommended)")]
        public int instanceID { get; set; } = 0;

        [JsonInclude, JsonPropertyName("path")]
        [Description("Path of a GameObject in the hierarchy Sample 'character/hand/finger/particle'. Priority: 2.")]
        public string? path { get; set; } = null;

        [JsonInclude, JsonPropertyName("name")]
        [Description("Name of a GameObject. Priority: 3.")]
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
            return "GameObject unknown";
        }
    }
}