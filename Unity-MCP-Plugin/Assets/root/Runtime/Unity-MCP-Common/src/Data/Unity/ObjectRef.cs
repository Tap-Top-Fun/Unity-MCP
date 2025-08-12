using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace com.IvanMurzak.ReflectorNet.Model.Unity
{
    [System.Serializable]
    [Description("Reference to UnityEngine.Object instance. It could be GameObject, Component, Asset, etc. Anything extended from UnityEngine.Object.")]
    public class ObjectRef
    {
        public static class Property
        {
            public const string InstanceID = "instanceID";
        }
        [JsonInclude, JsonPropertyName(Property.InstanceID)]
        [Description("Instance ID of the UnityEngine.Object. If this is '0', then it will be used as 'null'.")]
        public virtual int instanceID { get; set; } = 0;

        public ObjectRef() : this(id: 0) { }
        public ObjectRef(int id) => instanceID = id;

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            if (instanceID != 0)
                stringBuilder.Append($"{Property.InstanceID}={instanceID}");

            if (stringBuilder.Length == 0)
                return $"{Property.InstanceID}={instanceID}";

            return stringBuilder.ToString();
        }
    }
}