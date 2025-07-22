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
        protected override StringBuilder? ModifyProperty(Reflector reflector, ref object obj, SerializedMember property, int depth = 0, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            var material = obj as Material;
            var propType = TypeUtils.GetType(property.typeName);
            if (propType == null)
                return stringBuilder.AppendLine($"{padding}[Error] Property type '{property.typeName}' not found. Convertor: {GetType().Name}");

            switch (propType)
            {
                case Type t when t == typeof(int):
                    if (material.HasInt(property.name))
                    {
                        material.SetInt(property.name, property.GetValue<int>());
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{property.name}' modified to '{property.GetValue<int>()}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{property.name}' not found. Convertor: {GetType().Name}");
                case Type t when t == typeof(float):
                    if (material.HasFloat(property.name))
                    {
                        material.SetFloat(property.name, property.GetValue<float>());
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{property.name}' modified to '{property.GetValue<float>()}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{property.name}' not found. Convertor: {GetType().Name}");
                case Type t when t == typeof(Color):
                    if (material.HasColor(property.name))
                    {
                        material.SetColor(property.name, property.GetValue<Color>());
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{property.name}' modified to '{property.GetValue<Color>()}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{property.name}' not found. Convertor: {GetType().Name}");
                case Type t when t == typeof(Vector4):
                    if (material.HasVector(property.name))
                    {
                        material.SetVector(property.name, property.GetValue<Vector4>());
                        return stringBuilder.AppendLine($"{padding}[Success] Property '{property.name}' modified to '{property.GetValue<Vector4>()}'. Convertor: {GetType().Name}");
                    }
                    return stringBuilder.AppendLine($"{padding}[Error] Property '{property.name}' not found. Convertor: {GetType().Name}");
                // case Type t when t == typeof(Texture):
                //     if (material.HasTexture(property.name))
                //     {
                //         var instanceID = property.GetValue<InstanceID>()?.instanceID ?? property.GetValue<int>();
                //         var texture = instanceID == 0
                //             ? null
                //             : UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as Texture;
                //         material.SetTexture(property.name, texture);
                //         return stringBuilder.AppendLine($"{padding}[Success] Property '{property.name}' modified to '{texture?.name ?? "null"}'.");
                //     }
                //     return stringBuilder.AppendLine($"{padding}[Error] Property '{property.name}' not found.");
                default:
                    return stringBuilder.AppendLine($"{padding}[Error] Property type '{property.typeName}' is not supported. Convertor: {GetType().Name}");
            }
        }

        public override bool SetAsField(Reflector reflector, ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value, int depth = 0, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            stringBuilder?.AppendLine($"{padding}[Warning] Cannot set field '{value.name.ValueOrNull()}' for {type.FullName}. This type is not supported for setting values. Convertor: {GetType().Name}");
            return false;
        }

        public override bool SetAsProperty(Reflector reflector, ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value, int depth = 0, StringBuilder? stringBuilder = null,
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