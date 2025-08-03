#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#if !UNITY_EDITOR
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
            return null;
        }

        public override bool SetAsField(
            Reflector reflector,
            ref object? obj,
            Type fallbackType,
            FieldInfo fieldInfo,
            SerializedMember? value,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);

            if (logger?.IsEnabled(LogLevel.Trace) == true)
                logger.LogTrace($"{StringUtils.GetPadding(depth)}Set as field type='{fieldInfo.FieldType.GetTypeName(pretty: true)}'. Convertor='{GetType().GetTypeShortName()}'.");

            if (logger?.IsEnabled(LogLevel.Error) == true)
                logger.LogError($"{padding}Cannot set field '{value?.name.ValueOrNull()}' for object with type '{fallbackType.GetTypeName(pretty: false)}'. This type is not supported for setting values in runtime. Convertor: {GetType().GetTypeShortName()}");

            stringBuilder?.AppendLine($"{padding}[Error] Cannot set field '{value?.name.ValueOrNull()}' for {fallbackType.GetTypeName(pretty: false)}. This type is not supported for setting values in runtime. Convertor: {GetType().GetTypeShortName()}");
            return false;
        }

        public override bool SetAsProperty(
            Reflector reflector,
            ref object? obj,
            Type fallbackType,
            PropertyInfo propertyInfo,
            SerializedMember? value,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);

            if (logger?.IsEnabled(LogLevel.Trace) == true)
                logger.LogTrace($"{StringUtils.GetPadding(depth)}Set as property type='{propertyInfo.PropertyType.GetTypeName(pretty: true)}'. Convertor='{GetType().GetTypeShortName()}'.");

            if (logger?.IsEnabled(LogLevel.Error) == true)
                logger.LogError($"{padding}Cannot set property '{value?.name.ValueOrNull()}' for object with type '{fallbackType.GetTypeName(pretty: false)}'. This type is not supported for setting values in runtime. Convertor: {GetType().GetTypeShortName()}");

            stringBuilder?.AppendLine($"{padding}[Error] Cannot set property '{value?.name.ValueOrNull()}' for {fallbackType.GetTypeName(pretty: false)}. This type is not supported for setting values in runtime. Convertor: {GetType().GetTypeShortName()}");
            return false;
        }
    }
}
#endif