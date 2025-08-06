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
using com.IvanMurzak.Unity.MCP.Utils;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public class RS_UnityEngineObject : RS_UnityEngineObject<UnityEngine.Object> { }
    public partial class RS_UnityEngineObject<T> : RS_GenericUnity<T> where T : UnityEngine.Object
    {
        public override bool AllowCascadePropertiesConversion => false;
        public override bool AllowSetValue => false;

        protected override SerializedMember InternalSerialize(
            Reflector reflector,
            object? obj,
            Type type,
            string name = null,
            bool recursive = true,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            ILogger? logger = null)
        {
            if (obj == null)
                return SerializedMember.FromValue(reflector, type, value: null, name: name);

            var unityObject = obj as T;

            if (!type.IsClass)
                throw new ArgumentException($"Unsupported type: '{type.GetTypeName(pretty: false)}'. Convertor: {GetType().GetTypeShortName()}");

            if (recursive)
            {
                return new SerializedMember()
                {
                    name = name,
                    typeName = type.FullName,
                    fields = SerializeFields(
                        reflector,
                        obj: obj,
                        flags: flags,
                        depth: depth,
                        stringBuilder: stringBuilder,
                        logger: logger),
                    props = SerializeProperties(
                        reflector,
                        obj: obj,
                        flags: flags,
                        depth: depth,
                        stringBuilder: stringBuilder,
                        logger: logger)
                }.SetValue(reflector, new ObjectRef(unityObject?.GetInstanceID() ?? 0));
            }
            else
            {
                var objectRef = new ObjectRef(unityObject?.GetInstanceID() ?? 0);
                return SerializedMember.FromValue(reflector, type, objectRef, name);
            }
        }

        protected override bool SetValue(
            Reflector reflector,
            ref object obj,
            Type type,
            JsonElement? value,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);

            if (logger?.IsEnabled(LogLevel.Trace) == true)
                logger.LogTrace($"{padding}Set value type='{type.GetTypeName(pretty: true)}'. Convertor='{GetType().GetTypeShortName()}'.");

            stringBuilder?.AppendLine($"{padding}[Warning] Cannot set value for type '{type.GetTypeName(pretty: false)}'. This type is not supported for setting values. Maybe did you want to set a field or a property? If so, set the value in the '{nameof(SerializedMember.fields)}' or '{nameof(SerializedMember.props)}' property instead. Convertor: {GetType().GetTypeShortName()}");
            return false;
        }

        public override object? Deserialize(
            Reflector reflector,
            SerializedMember data,
            Type? fallbackType = null,
            string? fallbackName = null,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            ILogger? logger = null)
        {
            return data.valueJsonElement.ToObjectRef().FindObject();
        }

        protected override object? DeserializeValueAsJsonElement(
            Reflector reflector,
            SerializedMember data,
            Type type,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            ILogger? logger = null)
        {
            return data.valueJsonElement.ToObjectRef().FindObject();
        }
    }
}