using System.ComponentModel;
using System.Text.Json.Serialization;

namespace com.IvanMurzak.ReflectorNet.Model.Unity
{
    [Description(@"Component reference. Used to find a Component at GameObject.")]
    public class ComponentRef
    {
        [JsonInclude, JsonPropertyName("instanceID")]
        [Description("Component 'instanceID' (int). Priority: 1. (Recommended)")]
        public int instanceID { get; set; } = 0;

        [JsonInclude, JsonPropertyName("index")]
        [Description("Component 'index' attached to a gameObject. The first index is '0' and that is usually Transform or RectTransform. Priority: 2. Default value is -1.")]
        public int index { get; set; } = -1;

        [JsonInclude, JsonPropertyName("typeName")]
        [Description("Component type full name. Sample 'UnityEngine.Transform'. If the gameObject has two components of the same type, the output component is unpredictable. Priority: 3. Default value is null.")]
        public string? typeName { get; set; } = null;

        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                if (instanceID != 0)
                    return true;
                if (index >= 0)
                    return true;
                if (!string.IsNullOrEmpty(typeName))
                    return true;
                return false;
            }
        }

        public ComponentRef() { }
        public ComponentRef(int instanceID)
        {
            this.instanceID = instanceID;
        }

        public override string ToString()
        {
            if (instanceID != 0)
                return $"Component {nameof(instanceID)}='{instanceID}'";
            if (index >= 0)
                return $"Component {nameof(index)}='{index}'";
            if (!string.IsNullOrEmpty(typeName))
                return $"Component {nameof(typeName)}='{typeName}'";
            return "Component unknown";
        }
    }
}