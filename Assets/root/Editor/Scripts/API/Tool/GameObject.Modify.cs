#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.Unity.MCP.Common;
using com.IvanMurzak.ReflectorNet.Model.Unity;
using com.IvanMurzak.Unity.MCP.Utils;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.ReflectorNet;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_GameObject
    {
        [McpPluginTool
        (
            "GameObject_Modify",
            Title = "Modify GameObjects in opened Prefab or in a Scene"
        )]
        [Description(@"Modify GameObjects and/or attached component's field and properties.
You can modify multiple GameObjects at once. Just provide the same number of GameObject references and SerializedMember objects.")]
        public string Modify
        (
            GameObjectRefList gameObjectRefs,
            [Description("Each item in the array represents a GameObject modification of the 'gameObjectRefs' at the same index.\n" +
                "Usually a GameObject is a container for components. Each component may have fields and properties for modification.\n" +
                "If you need to modify components of a gameObject, please use '" + nameof(SerializedMember.fields) + "' to wrap a component into it. " +
                "Each component needs to have '" + nameof(SerializedMember.typeName) + "' and '" + nameof(SerializedMember.name) + "' or 'value." + nameof(GameObjectRef.instanceID) + "' fields to identify the exact modification target.\n" +
                "Ignore values that should not be modified.\n" +
                "Any unknown or wrong located fields and properties will be ignored.\n" +
                "Check the result of this command to see what was changed. The ignored fields and properties will be listed.")]
            SerializedMemberList gameObjectDiffs
        )
        => MainThread.Instance.Run(() =>
        {
            if (gameObjectRefs.Count == 0)
                return "[Error] No GameObject references provided. Please provide at least one GameObject reference.";

            if (gameObjectDiffs.Count != gameObjectRefs.Count)
                return $"[Error] The number of {nameof(gameObjectDiffs)} and {nameof(gameObjectRefs)} should be the same. " +
                    $"{nameof(gameObjectDiffs)}: {gameObjectDiffs.Count}, {nameof(gameObjectRefs)}: {gameObjectRefs.Count}";

            var stringBuilder = new StringBuilder();

            for (int i = 0; i < gameObjectRefs.Count; i++)
            {
                var go = GameObjectUtils.FindBy(gameObjectRefs[i], out var error);
                if (error != null)
                {
                    stringBuilder.AppendLine(error);
                    continue;
                }
                var objToModify = (object)go;
                var type = TypeUtils.GetType(gameObjectDiffs[i].typeName);
                if (typeof(UnityEngine.Component).IsAssignableFrom(type))
                {
                    var component = go.GetComponent(type);
                    if (component == null)
                    {
                        stringBuilder.AppendLine($"[Error] Component '{type.GetTypeName(pretty: false)}' not found on GameObject '{go.name.ValueOrNull()}'.");
                        continue;
                    }
                    objToModify = component;
                }
                Reflector.Instance.Populate(ref objToModify, gameObjectDiffs[i], objToModify.GetType(), stringBuilder: stringBuilder);
            }

            return stringBuilder.ToString();
        });
    }
}