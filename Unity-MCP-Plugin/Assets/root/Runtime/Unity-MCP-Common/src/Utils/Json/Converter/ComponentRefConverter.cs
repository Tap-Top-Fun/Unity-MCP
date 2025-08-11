using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using com.IvanMurzak.ReflectorNet.Model.Unity;

namespace com.IvanMurzak.Unity.MCP.Common.Json
{
    public class ComponentRefConverter : JsonConverter<ComponentRef>
    {
        public override ComponentRef? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object token.");

            var result = new ComponentRef();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return result;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read(); // Move to the value token

                    switch (propertyName)
                    {
                        case nameof(ComponentRef.instanceID):
                            result.instanceID = reader.GetInt32();
                            break;
                        case nameof(ComponentRef.index):
                            result.index = reader.GetInt32();
                            break;
                        case nameof(ComponentRef.typeName):
                            result.typeName = reader.GetString();
                            break;
                        default:
                            throw new JsonException($"Unexpected property name: {propertyName}. "
                                + $"Expected '{nameof(ComponentRef.instanceID)}', '{nameof(ComponentRef.index)}', or '{nameof(ComponentRef.typeName)}'.");
                    }
                }
            }

            throw new JsonException("Expected end of object token.");
        }

        public override void Write(Utf8JsonWriter writer, ComponentRef value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteStartObject();

                // Write the "instanceID" property
                writer.WritePropertyName(nameof(ComponentRef.instanceID));
                writer.WriteNumberValue(0);

                writer.WriteEndObject();
                return;
            }

            writer.WriteStartObject();

            writer.WriteNumber(nameof(ComponentRef.instanceID), value.instanceID);

            if (value.index != -1)
                writer.WriteNumber(nameof(ComponentRef.index), value.index);

            if (!string.IsNullOrEmpty(value.typeName))
                writer.WriteString(nameof(ComponentRef.typeName), value.typeName);

            writer.WriteEndObject();
        }
    }
}
