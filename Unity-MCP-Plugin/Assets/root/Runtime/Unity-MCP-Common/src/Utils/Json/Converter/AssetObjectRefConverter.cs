using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using com.IvanMurzak.ReflectorNet.Model.Unity;

namespace com.IvanMurzak.Unity.MCP.Common.Json
{
    public class AssetObjectRefConverter : JsonConverter<AssetObjectRef>
    {
        public override AssetObjectRef? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object token.");

            var assetObjectRef = new AssetObjectRef();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return assetObjectRef;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read(); // Move to the value token

                    switch (propertyName)
                    {
                        case nameof(AssetObjectRef.instanceID):
                            assetObjectRef.instanceID = reader.GetInt32();
                            break;
                        case nameof(AssetObjectRef.assetPath):
                            assetObjectRef.assetPath = reader.GetString();
                            break;
                        case nameof(AssetObjectRef.assetGuid):
                            assetObjectRef.assetGuid = reader.GetString();
                            break;
                        default:
                            throw new JsonException($"Unexpected property name: {propertyName}. "
                                + $"Expected '{nameof(AssetObjectRef.instanceID)}', '{nameof(AssetObjectRef.assetPath)}', or '{nameof(AssetObjectRef.assetGuid)}'.");
                    }
                }
            }

            throw new JsonException("Expected end of object token.");
        }

        public override void Write(Utf8JsonWriter writer, AssetObjectRef value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteStartObject();

                // Write the "instanceID" property
                writer.WritePropertyName(nameof(ObjectRef.instanceID));
                writer.WriteNumberValue(0);

                writer.WriteEndObject();
                return;
            }

            writer.WriteStartObject();

            // Write the "instanceID" property
            writer.WritePropertyName(nameof(AssetObjectRef.instanceID));
            writer.WriteNumberValue(value.instanceID);

            // Write the "assetPath" property
            if (!string.IsNullOrEmpty(value.assetPath))
            {
                writer.WritePropertyName(nameof(AssetObjectRef.assetPath));
                writer.WriteStringValue(value.assetPath);
            }

            // Write the "assetGuid" property
            if (!string.IsNullOrEmpty(value.assetGuid))
            {
                writer.WritePropertyName(nameof(AssetObjectRef.assetGuid));
                writer.WriteStringValue(value.assetGuid);
            }

            writer.WriteEndObject();
        }
    }
}