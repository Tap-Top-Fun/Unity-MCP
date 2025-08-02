#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Text;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor;
using com.IvanMurzak.Unity.MCP.Utils;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineObject<T> : RS_GenericUnity<T> where T : UnityEngine.Object
    {
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

        public override bool SetAsField(
            Reflector reflector,
            ref object? obj,
            Type type,
            FieldInfo fieldInfo,
            SerializedMember? value,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            ILogger? logger = null)
        {
            if (logger?.IsEnabled(LogLevel.Trace) == true)
                logger.LogTrace($"{StringUtils.GetPadding(depth)}Set as field type='{type.GetTypeName(pretty: true)}'. Convertor='{GetType().Name}'.");

            var padding = StringUtils.GetPadding(depth);
            var refObj = value.valueJsonElement.ToObjectRef().FindObject();
            stringBuilder?.AppendLine($"{padding}[Success] Field '{value.name.ValueOrNull()}' modified to '{refObj?.GetInstanceID() ?? 0}'. Convertor: {GetType().Name}");

            fieldInfo.SetValue(obj, refObj);
            return true;
        }

        public override bool SetAsProperty(
            Reflector reflector,
            ref object? obj,
            Type type,
            PropertyInfo propertyInfo,
            SerializedMember? value,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            var refObj = value.valueJsonElement.ToObjectRef().FindObject();
            stringBuilder?.AppendLine($"{padding}[Success] Property '{value.name.ValueOrNull()}' modified to '{refObj?.GetInstanceID()}'. Convertor: {GetType().Name}");

            propertyInfo.SetValue(obj, refObj);
            return true;
        }
    }
}
#endif