#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Model.Unity;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsSerializedMember
    {
        public static bool TryGetInstanceID(this SerializedMember member, out int instanceID)
        {
            try
            {
                var objectRef = member.GetValue<ObjectRef>();
                if (objectRef != null)
                {
                    instanceID = objectRef.instanceID;
                    return true;
                }
            }
            catch
            {
                // Ignore exceptions, fallback to instanceID field
            }

            try
            {
                var fieldValue = member.GetField(nameof(ObjectRef.instanceID));
                if (fieldValue != null)
                {
                    instanceID = fieldValue.GetValue<int>();
                    return true;
                }
            }
            catch
            {
                // Ignore exceptions, fallback to instanceID field
            }

            instanceID = 0;
            return false;
        }
        public static bool TryGetGameObjectInstanceID(this SerializedMember member, out int instanceID)
        {
            try
            {
                var objectRef = member.GetValue<GameObjectRef>();
                if (objectRef != null)
                {
                    instanceID = objectRef.instanceID;
                    return true;
                }
            }
            catch
            {
                // Ignore exceptions, fallback to instanceID field
            }

            try
            {
                var fieldValue = member.GetField(nameof(GameObjectRef.instanceID));
                if (fieldValue != null)
                {
                    instanceID = fieldValue.GetValue<int>();
                    return true;
                }
            }
            catch
            {
                // Ignore exceptions, fallback to instanceID field
            }

            instanceID = 0;
            return false;
        }
        public static bool TryGetGameObjectPath(this SerializedMember member, out string? path)
        {
            try
            {
                var objectRef = member.GetValue<GameObjectRef>();
                if (objectRef != null)
                {
                    path = objectRef.path;
                    return true;
                }
            }
            catch
            {
                // Ignore exceptions, fallback to instanceID field
            }

            try
            {
                var fieldValue = member.GetField(nameof(GameObjectRef.path));
                if (fieldValue != null)
                {
                    path = fieldValue.GetValue<string>();
                    return true;
                }
            }
            catch
            {
                // Ignore exceptions, fallback to instanceID field
            }

            path = null;
            return false;
        }
    }
}