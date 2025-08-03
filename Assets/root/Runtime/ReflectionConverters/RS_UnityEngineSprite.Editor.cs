#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEditor;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace com.IvanMurzak.Unity.MCP.Reflection.Convertor
{
    public partial class RS_UnityEngineSprite : RS_UnityEngineObject<UnityEngine.Sprite>
    {
        public override StringBuilder Populate(
            Reflector reflector,
            ref object obj,
            SerializedMember data,
            Type? dataType = null,
            int depth = 0,
            StringBuilder stringBuilder = null,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            ILogger? logger = null)
        {
            var padding = StringUtils.GetPadding(depth);

            if (logger?.IsEnabled(LogLevel.Trace) == true)
                logger.LogTrace($"{StringUtils.GetPadding(depth)}Populate sprite from data. Convertor='{GetType().GetTypeShortName()}'.");

            if (!data.TryGetInstanceID(out var instanceID))
            {
                if (logger?.IsEnabled(LogLevel.Error) == true)
                    logger.LogError($"{padding}InstanceID not found. Set 'instanceID` as 0 if you want to set it to null. Convertor: {GetType().GetTypeShortName()}");

                return stringBuilder?.AppendLine($"{padding}[Error] InstanceID not found. Set 'instanceID` as 0 if you want to set it to null. Convertor: {GetType().GetTypeShortName()}");
            }
            if (instanceID == 0)
            {
                obj = null;
                return stringBuilder?.AppendLine($"{padding}[Success] InstanceID is 0. Cleared the reference. Convertor: {GetType().GetTypeShortName()}");
            }
            var textureOrSprite = EditorUtility.InstanceIDToObject(instanceID);
            if (textureOrSprite == null)
            {
                if (logger?.IsEnabled(LogLevel.Error) == true)
                    logger.LogError($"{padding}InstanceID {instanceID} not found. Convertor: {GetType().GetTypeShortName()}");

                return stringBuilder?.AppendLine($"{padding}[Error] InstanceID {instanceID} not found. Convertor: {GetType().GetTypeShortName()}");
            }

            if (textureOrSprite is UnityEngine.Texture2D texture)
            {
                var path = AssetDatabase.GetAssetPath(texture);
                var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(path)
                    .OfType<UnityEngine.Sprite>()
                    .ToArray();
                if (sprites.Length == 0)
                {
                    if (logger?.IsEnabled(LogLevel.Error) == true)
                        logger.LogError($"{padding}No sprites found for texture at path: {path}. Convertor: {GetType().GetTypeShortName()}");

                    return stringBuilder?.AppendLine($"{padding}[Error] No sprites found for texture at path: {path}. Convertor: {GetType().GetTypeShortName()}");
                }

                obj = sprites[0]; // Assign the first sprite found
                return stringBuilder?.AppendLine($"{padding}[Success] Assigned sprite from texture: {path}. Convertor: {GetType().GetTypeShortName()}");
            }
            if (textureOrSprite is UnityEngine.Sprite sprite)
            {
                obj = sprite;
                return stringBuilder?.AppendLine($"{padding}[Success] Assigned sprite: {sprite.name}. Convertor: {GetType().GetTypeShortName()}");
            }

            if (logger?.IsEnabled(LogLevel.Error) == true)
                logger.LogError($"{padding}InstanceID {instanceID} is not a Texture2D or Sprite. Convertor: {GetType().GetTypeShortName()}");

            return stringBuilder?.AppendLine($"{padding}[Error] InstanceID {instanceID} is not a Texture2D or Sprite. Convertor: {GetType().GetTypeShortName()}");
        }
    }
}
#endif