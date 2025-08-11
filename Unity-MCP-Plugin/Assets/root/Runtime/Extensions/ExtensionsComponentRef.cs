#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;

namespace com.IvanMurzak.Unity.MCP.Utils
{
    public static class ExtensionsComponentRef
    {
        public static bool Matches(this ComponentRef componentRef, UnityEngine.Component component, int? index = null)
        {
            if (componentRef.instanceID != 0)
            {
                return componentRef.instanceID == (component?.GetInstanceID() ?? 0);
            }
            if (componentRef.index >= 0 && index != null)
            {
                return componentRef.index == index.Value;
            }
            if (!StringUtils.IsNullOrEmpty(componentRef.typeName))
            {
                var type = component?.GetType() ?? typeof(UnityEngine.Component);
                return type.IsMatch(componentRef.typeName);
            }
            if (componentRef.instanceID == 0 && component == null)
            {
                return true; // Matches null component
            }
            return false;
        }
    }
}