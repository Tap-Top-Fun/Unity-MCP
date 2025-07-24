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
using ILogger = Microsoft.Extensions.Logging.ILogger;

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

        protected override IEnumerable<string> GetIgnoredProperties()
        {
            foreach (var property in base.GetIgnoredProperties())
                yield return property;

            yield return nameof(UnityEngine.GameObject.gameObject);
            yield return nameof(UnityEngine.GameObject.transform);
            yield return nameof(UnityEngine.GameObject.scene);
        }
        protected override SerializedMember InternalSerialize(Reflector reflector, object obj, Type type, string name = null, bool recursive = true,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            var unityObject = obj as UnityEngine.GameObject;
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
                var objectRef = new ObjectRef(unityObject.GetInstanceID());
                return SerializedMember.FromValue(type, objectRef, name);
            }
        }

        protected override List<SerializedMember> SerializeFields(Reflector reflector, object obj, BindingFlags flags, ILogger? logger = null)
        {
            var serializedFields = base.SerializeFields(reflector, obj, flags, logger: logger) ?? new();

            var go = obj as UnityEngine.GameObject;
            var components = go.GetComponents<UnityEngine.Component>();

            serializedFields.Capacity += components.Length;

            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                var componentType = component?.GetType() ?? typeof(UnityEngine.Component);
                var componentSerialized = reflector.Serialize(
                    obj: component,
                    type: componentType,
                    name: GetComponentName(i),
                    recursive: true,
                    flags: flags,
                    logger: logger
                );
                serializedFields.Add(componentSerialized);
            }
            return serializedFields;
        }

        protected override bool SetValue(Reflector reflector, ref object obj, Type type, JsonElement? value, int depth = 0, StringBuilder? stringBuilder = null, ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            stringBuilder?.AppendLine($"{padding}[Warning] Cannot set value for '{type.GetTypeName(pretty: false)}'. This type is not supported for setting values. Maybe did you want to set a field or a property? If so, set the value in the '{nameof(SerializedMember.fields)}' or '{nameof(SerializedMember.props)}' property instead.");
            return false;
        }

        protected override StringBuilder? ModifyField(Reflector reflector, ref object obj, SerializedMember fieldValue, int depth = 0, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            var go = obj as UnityEngine.GameObject;

            var type = TypeUtils.GetType(fieldValue.typeName);
            if (type == null)
                return stringBuilder?.AppendLine($"{padding}[Error] Type not found: '{fieldValue.typeName}'");

            // If not a component, use base method
            if (!typeof(UnityEngine.Component).IsAssignableFrom(type))
                return base.ModifyField(reflector, ref obj, fieldValue, depth: depth, stringBuilder: stringBuilder, flags: flags);

            TryParseComponentIndex(fieldValue.name, out var index);

            var componentInstanceID = fieldValue.GetInstanceID();
            if (componentInstanceID == 0 && index == -1)
                return stringBuilder?.AppendLine($"{padding}[Error] Component 'instanceID' is not provided. Use 'instanceID' or name '[index]' to specify the component. '{fieldValue.name}' is not valid.");

            var allComponents = go.GetComponents<UnityEngine.Component>();
            var component = componentInstanceID == 0
                ? index >= 0 && index < allComponents.Length
                    ? allComponents[index]
                    : null
                : allComponents.FirstOrDefault(c => c.GetInstanceID() == componentInstanceID);

            if (component == null)
                return stringBuilder?.AppendLine($"{padding}[Error] Component not found. Use 'instanceID' or name 'component_[index]' to specify the component.");

            var componentObject = (object)component;
            return reflector.Populate(ref componentObject, fieldValue, depth: depth, stringBuilder: stringBuilder, logger: logger);
        }
    }
}