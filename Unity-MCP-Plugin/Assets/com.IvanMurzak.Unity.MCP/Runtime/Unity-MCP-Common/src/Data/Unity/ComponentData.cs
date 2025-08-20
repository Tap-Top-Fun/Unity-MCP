#nullable enable
using System.Collections.Generic;

namespace com.IvanMurzak.ReflectorNet.Model.Unity
{
    [System.Serializable]
    public class ComponentData : ComponentDataLight
    {
        public List<SerializedMember?>? fields { get; set; }
        public List<SerializedMember?>? properties { get; set; }

        public ComponentData() { }
    }
}