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
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineGameObject : RS_GenericUnity<UnityEngine.GameObject>
    {
        public override bool SetAsField(Reflector reflector, ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value, int depth = 0, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            var refObj = value.valueJsonElement.ToObjectRef().FindObject();

            // Validate if refObj is castable to the fieldInfo type
            var castable = fieldInfo.FieldType.IsAssignableFrom(refObj?.GetType() ?? typeof(UnityEngine.Object));
            if (!castable)
            {
                stringBuilder?.AppendLine($"{padding}[Error] Cannot set field '{value.name.ValueOrNull()}' for object with type '{type.GetTypeName(pretty: false)}'. Because the provided value with type '{refObj?.GetType().GetTypeName(pretty: false)}' is not assignable to field type '{fieldInfo.FieldType.GetTypeName(pretty: false)}'. Convertor: {GetType().Name}");
                return false;
            }

            fieldInfo.SetValue(obj, refObj);
            stringBuilder?.AppendLine($"{padding}[Success] Field '{value.name.ValueOrNull()}' modified to '{refObj?.GetInstanceID()}'. Convertor: {GetType().Name}");
            return true;
        }

        public override bool SetAsProperty(Reflector reflector, ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value, int depth = 0, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            var refObj = value.valueJsonElement.ToObjectRef().FindObject();

            // Validate if refObj is castable to the propertyInfo type
            var castable = propertyInfo.PropertyType.IsAssignableFrom(refObj?.GetType() ?? typeof(UnityEngine.Object));
            if (!castable)
            {
                stringBuilder?.AppendLine($"{padding}[Error] Cannot set property '{value.name.ValueOrNull()}' for object with type '{type.GetTypeName(pretty: false)}'. Because the provided value with type '{refObj?.GetType().GetTypeName(pretty: false)}' is not assignable to property type '{propertyInfo.PropertyType.GetTypeName(pretty: false)}'. Convertor: {GetType().Name}");
                return false;
            }

            stringBuilder?.AppendLine($"{padding}[Success] Property '{value.name.ValueOrNull()}' modified to '{refObj?.GetInstanceID()}'. Convertor: {GetType().Name}");
            propertyInfo.SetValue(obj, refObj);
            return true;
        }
    }
}
#endif