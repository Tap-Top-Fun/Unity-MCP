#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class RS_UnityEngineGameObject : RS_GenericUnity<UnityEngine.GameObject>
    {
        const string ComponentNamePrefix = "component_";
        static string GetComponentName(int index) => $"{ComponentNamePrefix}{index}";
        static bool TryParseComponentIndex(string name, out int index)
        {
            index = -1;
            if (string.IsNullOrEmpty(name) || !name.StartsWith(ComponentNamePrefix))
                return false;

            var indexStr = name.Substring(ComponentNamePrefix.Length).Trim('[', ']');
            return int.TryParse(indexStr, out index);
        }

        public override bool AllowSetValue => false;

        protected override IEnumerable<string> GetIgnoredProperties()
        {
            foreach (var property in base.GetIgnoredProperties())
                yield return property;

            yield return nameof(UnityEngine.GameObject.gameObject);
            yield return nameof(UnityEngine.GameObject.transform);
            yield return nameof(UnityEngine.GameObject.scene);
        }
        protected override SerializedMember InternalSerialize(
            Reflector reflector,
            object? obj,
            Type type,
            string? name = null,
            bool recursive = true,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            int depth = 0, StringBuilder? stringBuilder = null,
            ILogger? logger = null)
        {
            if (obj == null)
                return SerializedMember.FromValue(type, value: null, name: name);

            var unityObject = obj as UnityEngine.GameObject;
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
                }.SetValue(new GameObjectRef(unityObject.GetInstanceID()));
            }
            else
            {
                var objectRef = new GameObjectRef(unityObject.GetInstanceID());
                return SerializedMember.FromValue(type, objectRef, name);
            }
        }

        protected override SerializedMemberList SerializeFields(
            Reflector reflector,
            object obj,
            BindingFlags flags,
            int depth = 0,
            StringBuilder? stringBuilder = null,
            ILogger? logger = null)
        {
            var serializedFields = base.SerializeFields(
                reflector,
                obj: obj,
                flags: flags,
                depth: depth,
                stringBuilder: stringBuilder,
                logger: logger) ?? new();

            var go = obj as UnityEngine.GameObject;
            var components = go.GetComponents<UnityEngine.Component>();

            serializedFields.Capacity += components.Length;

            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var componentSerialized = reflector.Serialize(
                    obj: component,
                    fallbackType: typeof(UnityEngine.Component),
                    name: GetComponentName(i),
                    recursive: true,
                    flags: flags,
                    depth: depth + 1,
                    stringBuilder: stringBuilder,
                    logger: logger
                );
                serializedFields.Add(componentSerialized);
            }
            return serializedFields;
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
            if (logger?.IsEnabled(LogLevel.Trace) == true)
                logger.LogTrace($"{StringUtils.GetPadding(depth)}Set value type='{type.GetTypeName(pretty: true)}'. Convertor='{GetType().Name}'.");

            var padding = StringUtils.GetPadding(depth);
            stringBuilder?.AppendLine($"{padding}[Warning] Cannot set value for '{type.GetTypeName(pretty: false)}'. This type is not supported for setting values. Maybe did you want to set a field or a property? If so, set the value in the '{nameof(SerializedMember.fields)}' or '{nameof(SerializedMember.props)}' property instead.");
            return false;
        }

        protected override StringBuilder? PopulateField(
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
            var go = obj as UnityEngine.GameObject;

            // it is fine if type is unknown here, we will try to find the component by name or index
            var type = TypeUtils.GetType(fieldValue.typeName);

            int? instanceID = fieldValue.TryGetGameObjectInstanceID(out var tempInstanceID)
                ? tempInstanceID
                : null;

            int? index = TryParseComponentIndex(fieldValue.name, out var tempIndex)
                ? tempIndex
                : null;

            var component = GetComponent(go, instanceID, index, type, out var error);
            if (component == null)
            {
                if (type == null)
                {
                    if (logger?.IsEnabled(LogLevel.Error) == true)
                        logger.LogError($"{padding}[Error] Type not found for field '{fieldValue.name}' with type name '{fieldValue.typeName}'.");

                    return stringBuilder?.AppendLine($"{padding}[Error] Type not found: '{fieldValue.typeName}'");
                }

                // If not a component, use base method
                if (!typeof(UnityEngine.Component).IsAssignableFrom(type))
                {
                    return base.PopulateField(
                        reflector,
                        obj: ref obj,
                        objType: objType,
                        fieldValue: fieldValue,
                        depth: depth,
                        stringBuilder: stringBuilder,
                        flags: flags,
                        logger: logger);
                }

                return stringBuilder?.AppendLine($"{padding}[Error] {error}");
            }

            var componentObject = (object)component;
            return reflector.Populate(
                ref componentObject,
                data: fieldValue,
                fallbackObjType: type,
                depth: depth,
                flags: flags,
                stringBuilder: stringBuilder,
                logger: logger);
        }

        protected virtual UnityEngine.Component GetComponent(UnityEngine.GameObject go, int? instanceID, int? index, Type? typeName, out string? error)
        {
            var allComponents = go.GetComponents<UnityEngine.Component>();
            if (instanceID.HasValue && instanceID.Value != 0)
            {
                var component = allComponents.FirstOrDefault(c => c.GetInstanceID() == instanceID.Value);
                if (component != null)
                {
                    error = null;
                    return component;
                }
                error = $"Component with '{nameof(instanceID)}'='{instanceID.Value}' not found.";
                return null;
            }
            if (index.HasValue)
            {
                if (index < 0 || index >= allComponents.Length)
                {
                    error = $"Component with '{nameof(index)}'='{index.Value}' not found. Index is out of range.";
                    return null;
                }
                error = null;
                return allComponents[index.Value];
            }
            if (typeName != null)
            {
                var component = allComponents.FirstOrDefault(c => c.GetType() == typeName);
                if (component != null)
                {
                    error = null;
                    return component;
                }
                error = $"Component of type '{typeName.GetTypeName(pretty: false)}' not found.";
                return null;
            }
            error = $"No valid criteria provided to find the component. Use '{nameof(instanceID)}', '{nameof(index)}', or '{nameof(typeName)}'.";
            return null;
        }
    }
}