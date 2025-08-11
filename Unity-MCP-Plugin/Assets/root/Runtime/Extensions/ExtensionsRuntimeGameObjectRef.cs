#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.ReflectorNet.Model.Unity;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsRuntimeGameObjectRef
    {
        public static UnityEngine.GameObject? FindGameObject(this GameObjectRef? objectRef)
        {
            if (objectRef == null)
                return null;

            return GameObjectUtils.FindBy(objectRef, out string error);
        }
        public static GameObjectRef? ToGameObjectRef(this UnityEngine.GameObject? obj)
        {
            if (obj == null)
                return new GameObjectRef();

            return new GameObjectRef(obj.GetInstanceID());
        }
    }
}