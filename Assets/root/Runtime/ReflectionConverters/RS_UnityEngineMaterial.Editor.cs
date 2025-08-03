#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Text;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineMaterial : RS_GenericUnity<Material>
    {
        protected override StringBuilder? PopulateProperty(
            Reflector reflector,
            ref object? obj,
            Type objType,
            SerializedMember propertyValue,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);

            if (logger?.IsEnabled(LogLevel.Trace) == true)
                logger.LogTrace($"{StringUtils.GetPadding(depth)}PopulateProperty property='{propertyValue.name}' type='{propertyValue.typeName}'. Convertor='{GetType().Name}'.");

            var material = obj as Material;
            var propType = TypeUtils.GetType(propertyValue.typeName);
            if (propType == null)
            {
                if (logger?.IsEnabled(LogLevel.Error) == true)
                    logger.LogError($"{padding}Property type '{propertyValue.typeName}' not found. Convertor: {GetType().Name}");

                return stringBuilder?.AppendLine($"{padding}[Error] Property type '{propertyValue.typeName}' not found. Convertor: {GetType().Name}");
            }

            switch (propType)
            {
                case Type t when t == typeof(int):
                    if (material.HasInt(propertyValue.name))
                    {
                        material.SetInt(propertyValue.name, propertyValue.GetValue<int>(Reflector.Instance));
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{propertyValue.name}' modified to '{propertyValue.GetValue<int>(Reflector.Instance)}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{propertyValue.name}' not found. Convertor: {GetType().Name}");
                case Type t when t == typeof(float):
                    if (material.HasFloat(propertyValue.name))
                    {
                        material.SetFloat(propertyValue.name, propertyValue.GetValue<float>(Reflector.Instance));
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{propertyValue.name}' modified to '{propertyValue.GetValue<float>(Reflector.Instance)}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{propertyValue.name}' not found. Convertor: {GetType().Name}");
                case Type t when t == typeof(Color):
                    if (material.HasColor(propertyValue.name))
                    {
                        material.SetColor(propertyValue.name, propertyValue.GetValue<Color>(Reflector.Instance));
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{propertyValue.name}' modified to '{propertyValue.GetValue<Color>(Reflector.Instance)}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{propertyValue.name}' not found. Convertor: {GetType().Name}");
                case Type t when t == typeof(Vector4):
                    if (material.HasVector(propertyValue.name))
                    {
                        material.SetVector(propertyValue.name, propertyValue.GetValue<Vector4>(Reflector.Instance));
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{propertyValue.name}' modified to '{propertyValue.GetValue<Vector4>(Reflector.Instance)}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{propertyValue.name}' not found. Convertor: {GetType().Name}");
                case Type t when t == typeof(Texture):
                    if (material.HasTexture(propertyValue.name))
                    {
                        var objTexture = propertyValue.GetValue<ObjectRef>(Reflector.Instance).FindObject();
                        var texture = objTexture as Texture;
                        material.SetTexture(propertyValue.name, texture);
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{propertyValue.name}' modified to '{texture?.name ?? "null"}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{propertyValue.name}' not found. Convertor: {GetType().Name}");
                default:
                    if (logger?.IsEnabled(LogLevel.Error) == true)
                        logger.LogError($"{padding}Property type '{propertyValue.typeName}' is not supported. Convertor: {GetType().Name}");

                    return stringBuilder?.AppendLine($"{padding}[Error] Property type '{propertyValue.typeName}' is not supported. Convertor: {GetType().Name}");
            }
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
                logger.LogTrace($"{StringUtils.GetPadding(depth)}Set as field type='{fieldInfo.FieldType.GetTypeName(pretty: true)}'. Convertor='{GetType().Name}'.");

            var refObj = value?.valueJsonElement.ToObjectRef().FindObject();

            // Validate if refObj is castable to the fieldInfo type
            var castable = fieldInfo.FieldType.IsAssignableFrom(refObj?.GetType() ?? typeof(UnityEngine.Material));
            if (!castable)
            {
                if (logger?.IsEnabled(LogLevel.Error) == true)
                    logger.LogError($"{padding}Cannot set field '{value?.name.ValueOrNull()}' for object with type '{fallbackType.GetTypeName(pretty: false)}'. Because the provided value with type '{refObj?.GetType().GetTypeName(pretty: false)}' is not assignable to field type '{fieldInfo.FieldType.GetTypeName(pretty: false)}'. Convertor: {GetType().Name}");

                stringBuilder?.AppendLine($"{padding}[Error] Cannot set field '{value?.name.ValueOrNull()}' for object with type '{fallbackType.GetTypeName(pretty: false)}'. Because the provided value with type '{refObj?.GetType().GetTypeName(pretty: false)}' is not assignable to field type '{fieldInfo.FieldType.GetTypeName(pretty: false)}'. Convertor: {GetType().Name}");
                return false;
            }

            fieldInfo.SetValue(obj, refObj);
            stringBuilder?.AppendLine($"{padding}[Success] Field '{value?.name.ValueOrNull()}' modified to instanceID='{refObj?.GetInstanceID() ?? 0}'. Convertor: {GetType().Name}");
            return true;
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
                logger.LogTrace($"{StringUtils.GetPadding(depth)}Set as property type='{propertyInfo.PropertyType.GetTypeName(pretty: true)}'. Convertor='{GetType().Name}'.");

            var refObj = value?.valueJsonElement.ToObjectRef().FindObject();

            // Validate if refObj is castable to the propertyInfo type
            var castable = propertyInfo.PropertyType.IsAssignableFrom(refObj?.GetType() ?? typeof(UnityEngine.Material));
            if (!castable)
            {
                if (logger?.IsEnabled(LogLevel.Error) == true)
                    logger.LogError($"{padding}Cannot set property '{value?.name.ValueOrNull()}' for object with type '{fallbackType.GetTypeName(pretty: false)}'. Because the provided value with type '{refObj?.GetType().GetTypeName(pretty: false)}' is not assignable to property type '{propertyInfo.PropertyType.GetTypeName(pretty: false)}'. Convertor: {GetType().Name}");

                stringBuilder?.AppendLine($"{padding}[Error] Cannot set property '{value?.name.ValueOrNull()}' for object with type '{fallbackType.GetTypeName(pretty: false)}'. Because the provided value with type '{refObj?.GetType().GetTypeName(pretty: false)}' is not assignable to property type '{propertyInfo.PropertyType.GetTypeName(pretty: false)}'. Convertor: {GetType().Name}");
                return false;
            }

            propertyInfo.SetValue(obj, refObj);
            stringBuilder?.AppendLine($"{padding}[Success] Property '{value?.name.ValueOrNull()}' modified to instanceID='{refObj?.GetInstanceID() ?? 0}'. Convertor: {GetType().Name}");
            return true;
        }
    }
}
#endif