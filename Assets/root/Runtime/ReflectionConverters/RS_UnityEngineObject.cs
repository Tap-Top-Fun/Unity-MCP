#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public class RS_UnityEngineObject : RS_UnityEngineObject<UnityEngine.Object> { }
    public partial class RS_UnityEngineObject<T> : RS_GenericUnity<T> where T : UnityEngine.Object
    {
        protected override SerializedMember InternalSerialize(Reflector reflector, object obj, Type type, string name = null, bool recursive = true,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            var unityObject = obj as T;
            if (type.IsClass)
            {
                if (recursive)
                {
                    return new SerializedMember()
                    {
                        name = name,
                        typeName = type.FullName,
                        fields = SerializeFields(reflector, obj, flags, logger: logger),
                        props = SerializeProperties(reflector, obj, flags, logger: logger)
                    }.SetValue(new ObjectRef(unityObject.GetInstanceID()));
                }
                else
                {
                    var objectRef = new ObjectRef(unityObject?.GetInstanceID() ?? 0);
                    return SerializedMember.FromValue(type, objectRef, name);
                }
            }

            throw new ArgumentException($"Unsupported type: '{type.GetTypeName(pretty: false)}'. Convertor: {GetType().Name}");
        }

        protected override bool SetValue(Reflector reflector, ref object obj, Type type, JsonElement? value, int depth = 0, StringBuilder? stringBuilder = null, ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            stringBuilder?.AppendLine($"{padding}[Warning] Cannot set value for type '{type.GetTypeName(pretty: false)}'. This type is not supported for setting values. Maybe did you want to set a field or a property? If so, set the value in the '{nameof(SerializedMember.fields)}' or '{nameof(SerializedMember.props)}' property instead. Convertor: {GetType().Name}");
            return false;
        }
    }
}