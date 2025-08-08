#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.Text.Json;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model.Unity;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsJsonElement
    {
        public static GameObjectRef? ToGameObjectRef(this JsonElement? jsonElement, Reflector reflector, bool suppressException = true)
        {
            if (jsonElement == null)
                return null;

            if (suppressException)
                return JsonSerializer.Deserialize<GameObjectRef>(jsonElement.Value, reflector.JsonSerializerOptions);

            try
            {
                return JsonSerializer.Deserialize<GameObjectRef>(jsonElement.Value, reflector.JsonSerializerOptions);
            }
            catch
            {
                return null;
            }
        }
        public static ComponentRef? ToComponentRef(this JsonElement? jsonElement, Reflector reflector, bool suppressException = true)
        {
            if (jsonElement == null)
                return null;

            if (suppressException)
                return JsonSerializer.Deserialize<ComponentRef>(jsonElement.Value, reflector.JsonSerializerOptions);

            try
            {
                return JsonSerializer.Deserialize<ComponentRef>(jsonElement.Value, reflector.JsonSerializerOptions);
            }
            catch
            {
                return null;
            }
        }
        public static ObjectRef? ToObjectRef(this JsonElement? jsonElement, Reflector reflector, bool suppressException = true)
        {
            if (jsonElement == null)
                return null;

            if (suppressException)
                return JsonSerializer.Deserialize<ObjectRef>(jsonElement.Value, reflector.JsonSerializerOptions);

            try
            {
                return JsonSerializer.Deserialize<ObjectRef>(jsonElement.Value, reflector.JsonSerializerOptions);
            }
            catch
            {
                return null;
            }
        }
    }
}