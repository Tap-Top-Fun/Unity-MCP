using System.ComponentModel;
using System.Text.Json.Serialization;

namespace com.IvanMurzak.ReflectorNet.Model.Unity
{
    [System.Serializable]
    public class GameObjectRefID
    {
        [JsonInclude, JsonPropertyName("instanceID")]
        [Description("GameObject 'instanceID' (int). 0 means null.")]
        public int instanceID { get; set; }

        public GameObjectRefID() { }
    }
}