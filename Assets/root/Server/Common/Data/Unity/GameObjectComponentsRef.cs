#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;

namespace com.IvanMurzak.ReflectorNet.Model.Unity
{
    [System.Serializable]
    [Description(@"GameObject reference. Used to find GameObject in opened Prefab or in a Scene.
Use one of the following properties:
1. 'instanceID' (int) - recommended. It finds the exact GameObject.
2. 'path' (string) - finds GameObject by path. It may find a wrong GameObject.
3. 'name' (string) - finds GameObject by name. It may find a wrong GameObject.")]
    public class GameObjectComponentsRef : GameObjectRef
    {
        [Description("GameObject 'components'.")]
        public SerializedMemberList? components { get; set; }

        public GameObjectComponentsRef() { }

        public override string ToString()
        {
            var stringBuilder = new System.Text.StringBuilder();
            stringBuilder.AppendLine($"{base.ToString()}");
            if (components != null && components.Count > 0)
            {
                stringBuilder.AppendLine($"Components total amount: {components.Count}");
                for (int i = 0; i < components.Count; i++)
                    stringBuilder.AppendLine($"Component[{i}] {components[i]}");
            }
            else
            {
                stringBuilder.AppendLine("No Components");
            }
            return stringBuilder.ToString();
        }
    }
}