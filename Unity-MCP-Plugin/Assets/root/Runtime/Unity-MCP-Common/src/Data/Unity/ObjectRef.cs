using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace com.IvanMurzak.ReflectorNet.Model.Unity
{
    [System.Serializable]
    [Description("Reference to UnityEngine.Object instance. It could be GameObject, Component, Asset, etc. Anything extended from UnityEngine.Object.")]
    public class ObjectRef
    {
        [JsonInclude, JsonPropertyName("instanceID")]
        [Description("Instance ID of the UnityEngine.Object. If this is 0 and assetPath is not provided or empty or null, then it will be used as 'null'.")]
        public int instanceID;

        public ObjectRef() : this(id: 0) { }
        public ObjectRef(int id) => instanceID = id;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            if (instanceID != 0)
                stringBuilder.Append($"instanceID={instanceID}");

            if (stringBuilder.Length == 0)
                return $"instanceID={instanceID}";

            return stringBuilder.ToString();
        }
    }
}