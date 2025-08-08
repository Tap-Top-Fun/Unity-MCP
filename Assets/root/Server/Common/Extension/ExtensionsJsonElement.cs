#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Text.Json;
using com.IvanMurzak.ReflectorNet.Model.Unity;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsJsonElement
    {
        public static GameObjectRef? ToGameObjectRef(this JsonElement? jsonElement)
        {
            if (jsonElement == null)
                return null;
            try
            {
                return JsonSerializer.Deserialize<GameObjectRef>(jsonElement.Value);
            }
            catch
            {
                return null;
            }
        }
        public static ComponentRef? ToComponentRef(this JsonElement? jsonElement)
        {
            if (jsonElement == null)
                return null;
            try
            {
                return JsonSerializer.Deserialize<ComponentRef>(jsonElement.Value);
            }
            catch
            {
                return null;
            }
        }
        public static ObjectRef? ToObjectRef(this JsonElement? jsonElement)
        {
            if (jsonElement == null)
                return null;
            try
            {
                return JsonSerializer.Deserialize<ObjectRef>(jsonElement.Value);
            }
            catch
            {
                return null;
            }
        }
    }
}