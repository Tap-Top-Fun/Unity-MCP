#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#if !UNITY_EDITOR
using System;
using System.Reflection;
using System.Text;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
            var material = obj as Material;
            var propType = TypeUtils.GetType(propertyValue.typeName);
            if (propType == null)
                return stringBuilder.AppendLine($"{padding}[Error] Property type '{propertyValue.typeName}' not found. Convertor: {GetType().Name}");

            switch (propType)
            {
                case Type t when t == typeof(int):
                    if (material.HasInt(propertyValue.name))
                    {
                        material.SetInt(propertyValue.name, propertyValue.GetValue<int>());
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{propertyValue.name}' modified to '{propertyValue.GetValue<int>()}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{propertyValue.name}' not found. Convertor: {GetType().Name}");
                case Type t when t == typeof(float):
                    if (material.HasFloat(propertyValue.name))
                    {
                        material.SetFloat(propertyValue.name, propertyValue.GetValue<float>());
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{propertyValue.name}' modified to '{propertyValue.GetValue<float>()}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{propertyValue.name}' not found. Convertor: {GetType().Name}");
                case Type t when t == typeof(Color):
                    if (material.HasColor(propertyValue.name))
                    {
                        material.SetColor(propertyValue.name, propertyValue.GetValue<Color>());
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{propertyValue.name}' modified to '{propertyValue.GetValue<Color>()}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{propertyValue.name}' not found. Convertor: {GetType().Name}");
                case Type t when t == typeof(Vector4):
                    if (material.HasVector(propertyValue.name))
                    {
                        material.SetVector(propertyValue.name, propertyValue.GetValue<Vector4>());
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{propertyValue.name}' modified to '{propertyValue.GetValue<Vector4>()}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{propertyValue.name}' not found. Convertor: {GetType().Name}");
                // case Type t when t == typeof(Texture):
                //     if (material.HasTexture(property.name))
                //     {
                //         var instanceID = propertyValue.GetValue<InstanceID>()?.instanceID ?? propertyValue.GetValue<int>();
                //         var texture = instanceID == 0
                //             ? null
                //             : UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as Texture;
                //         material.SetTexture(propertyValue.name, texture);
                //         return stringBuilder.AppendLine($"{padding}[Success] Property '{propertyValue.name}' modified to '{texture?.name ?? "null"}'.");
                //     }
                //     return stringBuilder.AppendLine($"{padding}[Error] Property '{propertyValue.name}' not found.");
                default:
                    return stringBuilder.AppendLine($"{padding}[Error] Property type '{propertyValue.typeName}' is not supported. Convertor: {GetType().Name}");
            }
        }

        public override bool SetAsField(
            Reflector reflector,
            ref object obj,
            Type type,
            FieldInfo fieldInfo,
            SerializedMember? value,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            stringBuilder?.AppendLine($"{padding}[Warning] Cannot set field '{value.name.ValueOrNull()}' for {type.FullName}. This type is not supported for setting values. Convertor: {GetType().Name}");
            return false;
        }

        public override bool SetAsProperty(
            Reflector reflector,
            ref object obj,
            Type type,
            PropertyInfo propertyInfo,
            SerializedMember? value,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            stringBuilder?.AppendLine($"{padding}[Warning] Cannot set property '{value.name.ValueOrNull()}' for {type.FullName}. This type is not supported for setting values. Convertor: {GetType().Name}");
            return false;
        }
    }
}
#endif