#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Reflection;
using System.Text;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.ReflectorNet.Utils;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineSprite : RS_UnityEngineObject<UnityEngine.Sprite>
    {
        protected override SerializedMember InternalSerialize(Reflector reflector, object obj, Type type, string name = null, bool recursive = true,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            if (obj is UnityEngine.Texture2D texture)
            {
                var objectRef = new ObjectRef(texture.GetInstanceID());
                return SerializedMember.FromValue(type, objectRef, name);
            }

            return base.InternalSerialize(reflector, obj, type, name, recursive, flags);
        }
        public override bool SetAsField(Reflector reflector, ref object obj, Type type, FieldInfo fieldInfo, SerializedMember? value, int depth = 0, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            var currentValue = fieldInfo.GetValue(obj);
            Populate(reflector, ref currentValue, value, type, depth: depth, stringBuilder: stringBuilder, flags, logger);
            fieldInfo.SetValue(obj, currentValue);
            stringBuilder?.AppendLine($"{padding}[Success] Field '{value.name.ValueOrNull()}' modified to '{currentValue}'. Convertor: {GetType().Name}");
            return true;
        }
        public override bool SetAsProperty(Reflector reflector, ref object obj, Type type, PropertyInfo propertyInfo, SerializedMember? value, int depth = 0, StringBuilder? stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);
            var currentValue = propertyInfo.GetValue(obj);
            Populate(reflector, ref currentValue, value, type, depth: depth, stringBuilder: stringBuilder, flags, logger);
            propertyInfo.SetValue(obj, currentValue);
            stringBuilder?.AppendLine($"{padding}[Success] Property '{value.name.ValueOrNull()}' modified to '{currentValue}'. Convertor: {GetType().Name}");
            return true;
        }
    }
}