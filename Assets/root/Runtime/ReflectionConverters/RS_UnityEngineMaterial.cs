#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.Unity.MCP.Common.Reflection.Convertor;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;
using Microsoft.Extensions.Logging;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineMaterial : RS_UnityEngineObject<Material>
    {
        const string FieldShader = "shader";
        const string FieldName = "name";

        public override bool AllowCascadeSerialization => false;
        public override bool AllowSetValue => false;

        protected override SerializedMember InternalSerialize(Reflector reflector,
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
                return SerializedMember.FromValue(type, value: null, name: name);

            var padding = StringUtils.GetPadding(depth);

            var material = obj as Material;
            var shader = material.shader;
            int propertyCount = shader.GetPropertyCount();

            var properties = new SerializedMemberList(propertyCount);

            for (int i = 0; i < propertyCount; i++)
            {
                var propName = shader.GetPropertyName(i);
                var propType = shader.GetPropertyType(i) switch
                {
                    UnityEngine.Rendering.ShaderPropertyType.Int => typeof(int),
                    UnityEngine.Rendering.ShaderPropertyType.Float => typeof(float),
                    UnityEngine.Rendering.ShaderPropertyType.Range => typeof(float),
                    UnityEngine.Rendering.ShaderPropertyType.Color => typeof(Color),
                    UnityEngine.Rendering.ShaderPropertyType.Vector => typeof(Vector4),
                    UnityEngine.Rendering.ShaderPropertyType.Texture => typeof(Texture),
                    _ => throw new NotSupportedException($"Unsupported shader property type: '{shader.GetPropertyType(i)}'."
                        + " Supported types are: Int, Float, Range, Color, Vector, Texture.")
                };
                var propValue = shader.GetPropertyType(i) switch
                {
                    UnityEngine.Rendering.ShaderPropertyType.Int => material.GetInt(propName) as object,
                    UnityEngine.Rendering.ShaderPropertyType.Float => material.GetFloat(propName),
                    UnityEngine.Rendering.ShaderPropertyType.Range => material.GetFloat(propName),
                    UnityEngine.Rendering.ShaderPropertyType.Color => material.GetColor(propName),
                    UnityEngine.Rendering.ShaderPropertyType.Vector => material.GetVector(propName),
                    UnityEngine.Rendering.ShaderPropertyType.Texture => material.GetTexture(propName)?.GetInstanceID() != null
                        ? new ObjectRef(material.GetTexture(propName).GetInstanceID())
                        : null,
                    _ => throw new NotSupportedException($"Unsupported shader property type: '{shader.GetPropertyType(i)}'."
                        + " Supported types are: Int, Float, Range, Color, Vector, Texture.")
                };
                if (propType == null)
                {
                    if (logger?.IsEnabled(LogLevel.Warning) == true)
                        logger.LogWarning($"{padding}Material property '{propName}' has unsupported type '{shader.GetPropertyType(i)}'.");

                    if (stringBuilder != null)
                        stringBuilder.AppendLine($"{padding}[Warning] Material property '{propName}' has unsupported type '{shader.GetPropertyType(i)}'.");

                    continue;
                }
                properties.Add(SerializedMember.FromValue(propType, propValue, name: propName));
            }

            return new SerializedMember()
            {
                name = name,
                typeName = type.FullName,
                fields = new SerializedMemberList()
                {
                    SerializedMember.FromValue(name: FieldName, value: material.name),
                    SerializedMember.FromValue(name: FieldShader, value: shader.name)
                },
                props = properties,
            }.SetValue(new ObjectRef(material.GetInstanceID()));
        }

        protected override bool TryPopulateField(
            Reflector reflector,
            ref object obj,
            Type objType,
            SerializedMember fieldValue,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);

            if (logger?.IsEnabled(LogLevel.Trace) == true)
                logger.LogTrace($"{StringUtils.GetPadding(depth)}Populate field for type='{objType.GetTypeName(pretty: true)}'. Convertor='{GetType().GetTypeShortName()}'.");

            var material = obj as Material;

            if (fieldValue.name == FieldName)
            {
                material.name = fieldValue.GetValue<string>(Reflector.Instance);

                if (logger?.IsEnabled(LogLevel.Information) == true)
                    logger.LogInformation($"{padding}[Success] Material name set to '{material.name}'. Convertor: {GetType().GetTypeShortName()}");

                if (stringBuilder != null)
                    stringBuilder.AppendLine($"{padding}[Success] Material name set to '{material.name}'.");

                return true;
            }
            if (fieldValue.name == FieldShader)
            {
                var shaderName = fieldValue.GetValue<string>(Reflector.Instance);

                // Check if the shader is already set
                if (string.IsNullOrEmpty(shaderName) || material.shader.name == shaderName)
                {
                    if (logger?.IsEnabled(LogLevel.Information) == true)
                        logger.LogInformation($"{padding}Material '{material.name}' shader is already set to '{shaderName}'. Convertor: {GetType().GetTypeShortName()}");

                    if (stringBuilder != null)
                        stringBuilder.AppendLine($"{padding}[Info] Material '{material.name}' shader is already set to '{shaderName}'.");

                    return true;
                }

                var shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    if (logger?.IsEnabled(LogLevel.Error) == true)
                        logger.LogError($"{padding}[Error] Shader '{shaderName}' not found. Convertor: {GetType().GetTypeShortName()}");

                    if (stringBuilder != null)
                        stringBuilder.AppendLine($"{padding}[Error] Shader '{shaderName}' not found.");

                    return false;
                }

                material.shader = shader;

                if (logger?.IsEnabled(LogLevel.Information) == true)
                    logger.LogInformation($"{padding}[Success] Material '{material.name}' shader set to '{shaderName}'. Convertor: {GetType().GetTypeShortName()}");

                if (stringBuilder != null)
                    stringBuilder.AppendLine($"{padding}[Success] Material '{material.name}' shader set to '{shaderName}'.");

                return true;
            }

            if (logger?.IsEnabled(LogLevel.Error) == true)
                logger.LogError($"{padding}[Error] Field '{fieldValue.name}' is not supported for setting values in runtime. Convertor: {GetType().GetTypeShortName()}");

            if (stringBuilder != null)
                stringBuilder.AppendLine($"{padding}[Error] Field '{fieldValue.name}' is not supported for setting values in runtime. Convertor: {GetType().GetTypeShortName()}");

            return false;
        }

        public override object CreateInstance(Reflector reflector, Type type)
        {
            return new Material(Shader.Find("Standard"));
        }
    }
}