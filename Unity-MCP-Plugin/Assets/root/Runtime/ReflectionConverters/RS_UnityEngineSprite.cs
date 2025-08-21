#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Reflection;
using System.Text;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Utils;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineSprite : RS_UnityEngineObject<UnityEngine.Sprite>
    {
        public override bool AllowCascadeSerialization => false;

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

            if (obj is UnityEngine.Texture2D texture)
            {
                var objectRef = new ObjectRef(texture.GetInstanceID());
                return SerializedMember.FromValue(reflector, type, objectRef, name);
            }

            return base.InternalSerialize(reflector,
                obj: obj,
                type: type,
                name: name,
                recursive: recursive,
                flags: flags,
                depth: depth,
                stringBuilder: stringBuilder,
                logger: logger);
        }
        // public override bool SetAsField(
        //     Reflector reflector,
        //     ref object? obj,
        //     Type fallbackType,
        //     FieldInfo fieldInfo,
        //     SerializedMember? value,
        //     int depth = 0,
        //     StringBuilder? stringBuilder = null,
        //     BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
        //     ILogger? logger = null)
        // {
        //     var padding = StringUtils.GetPadding(depth);

        //     if (logger?.IsEnabled(LogLevel.Trace) == true)
        //         logger.LogTrace($"{StringUtils.GetPadding(depth)}Set as field type='{fieldInfo.FieldType.GetTypeName(pretty: true)}'. Convertor='{GetType().GetTypeShortName()}'.");

        //     var currentValue = fieldInfo.GetValue(obj);

        //     // Validate if the result will be castable to the fieldInfo type
        //     var refObj = value?.valueJsonElement.ToObjectRef().FindObject();
        //     var castable = fieldInfo.FieldType.IsAssignableFrom(refObj?.GetType() ?? typeof(UnityEngine.Sprite));
        //     if (!castable && refObj != null)
        //     {
        //         if (logger?.IsEnabled(LogLevel.Error) == true)
        //             logger.LogError($"{padding}Cannot set field '{value?.name.ValueOrNull()}' for object with type '{fallbackType.GetTypeName(pretty: false)}'. Because the provided value with type '{refObj?.GetType().GetTypeName(pretty: false)}' is not assignable to field type '{fieldInfo.FieldType.GetTypeName(pretty: false)}'. Convertor: {GetType().GetTypeShortName()}");

        //         stringBuilder?.AppendLine($"{padding}[Error] Cannot set field '{value?.name.ValueOrNull()}' for object with type '{fallbackType.GetTypeName(pretty: false)}'. Because the provided value with type '{refObj?.GetType().GetTypeName(pretty: false)}' is not assignable to field type '{fieldInfo.FieldType.GetTypeName(pretty: false)}'. Convertor: {GetType().GetTypeShortName()}");
        //         return false;
        //     }

        //     Populate(
        //         reflector: reflector,
        //         obj: ref currentValue,
        //         data: value,
        //         dataType: fallbackType,
        //         depth: depth,
        //         stringBuilder: stringBuilder,
        //         flags: flags,
        //         logger: logger);
        //     fieldInfo.SetValue(obj, currentValue);
        //     stringBuilder?.AppendLine($"{padding}[Success] Field '{value?.name.ValueOrNull()}' modified to '{currentValue}'. Convertor: {GetType().GetTypeShortName()}");
        //     return true;
        // }
        // public override bool SetAsProperty(
        //     Reflector reflector,
        //     ref object? obj,
        //     Type fallbackType,
        //     PropertyInfo propertyInfo,
        //     SerializedMember? value,
        //     int depth = 0,
        //     StringBuilder? stringBuilder = null,
        //     BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
        //     ILogger? logger = null)
        // {
        //     var padding = StringUtils.GetPadding(depth);

        //     if (logger?.IsEnabled(LogLevel.Trace) == true)
        //         logger.LogTrace($"{StringUtils.GetPadding(depth)}Set as property type='{propertyInfo.PropertyType.GetTypeName(pretty: true)}'. Convertor='{GetType().GetTypeShortName()}'.");

        //     var currentValue = propertyInfo.GetValue(obj);

        //     // Validate if the result will be castable to the propertyInfo type
        //     var refObj = value?.valueJsonElement.ToObjectRef().FindObject();
        //     var castable = propertyInfo.PropertyType.IsAssignableFrom(refObj?.GetType() ?? typeof(UnityEngine.Sprite));
        //     if (!castable && refObj != null)
        //     {
        //         if (logger?.IsEnabled(LogLevel.Error) == true)
        //             logger.LogError($"{padding}Cannot set property '{value?.name.ValueOrNull()}' for object with type '{fallbackType.GetTypeName(pretty: false)}'. Because the provided value with type '{refObj?.GetType().GetTypeName(pretty: false)}' is not assignable to property type '{propertyInfo.PropertyType.GetTypeName(pretty: false)}'. Convertor: {GetType().GetTypeShortName()}");

        //         stringBuilder?.AppendLine($"{padding}[Error] Cannot set property '{value?.name.ValueOrNull()}' for object with type '{fallbackType.GetTypeName(pretty: false)}'. Because the provided value with type '{refObj?.GetType().GetTypeName(pretty: false)}' is not assignable to property type '{propertyInfo.PropertyType.GetTypeName(pretty: false)}'. Convertor: {GetType().GetTypeShortName()}");
        //         return false;
        //     }

        //     Populate(
        //         reflector: reflector,
        //         obj: ref currentValue,
        //         data: value,
        //         dataType: fallbackType,
        //         depth: depth,
        //         stringBuilder: stringBuilder,
        //         flags: flags,
        //         logger: logger);
        //     propertyInfo.SetValue(obj, currentValue);
        //     stringBuilder?.AppendLine($"{padding}[Success] Property '{value?.name.ValueOrNull()}' modified to '{currentValue}'. Convertor: {GetType().GetTypeShortName()}");
        //     return true;
        // }
    }
}