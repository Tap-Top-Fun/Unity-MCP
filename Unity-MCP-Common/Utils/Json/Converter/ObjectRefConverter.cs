#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using com.IvanMurzak.ReflectorNet.Model.Unity;

namespace com.IvanMurzak.Unity.MCP.Common.Json
{
    public class ObjectRefConverter : JsonConverter<ObjectRef>
    {
        public override ObjectRef? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object token.");

            var objectRef = new ObjectRef();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return objectRef;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read(); // Move to the value token

                    switch (propertyName)
                    {
                        case nameof(ObjectRef.instanceID):
                            objectRef.instanceID = reader.GetInt32();
                            break;
                        case nameof(ObjectRef.assetPath):
                            objectRef.assetPath = reader.GetString();
                            break;
                        case nameof(ObjectRef.assetGuid):
                            objectRef.assetGuid = reader.GetString();
                            break;
                        default:
                            throw new JsonException($"Unexpected property name: {propertyName}. "
                                + $"Expected '{nameof(ObjectRef.instanceID)}', '{nameof(ObjectRef.assetPath)}', or '{nameof(ObjectRef.assetGuid)}'.");
                    }
                }
            }

            throw new JsonException("Expected end of object token.");
        }

        public override void Write(Utf8JsonWriter writer, ObjectRef value, JsonSerializerOptions options)
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
            writer.WritePropertyName(nameof(ObjectRef.instanceID));
            writer.WriteNumberValue(value.instanceID);

            // Write the "assetPath" property
            if (!string.IsNullOrEmpty(value.assetPath))
            {
                writer.WritePropertyName(nameof(ObjectRef.assetPath));
                writer.WriteStringValue(value.assetPath);
            }

            // Write the "assetGuid" property
            if (!string.IsNullOrEmpty(value.assetGuid))
            {
                writer.WritePropertyName(nameof(ObjectRef.assetGuid));
                writer.WriteStringValue(value.assetGuid);
            }

            writer.WriteEndObject();
        }
    }
}