using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using com.IvanMurzak.ReflectorNet;
using com.IvanMurzak.ReflectorNet.Json;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Common.Json.Converters
{
    public class Vector2IntConverter : JsonConverter<Vector2Int>, IJsonSchemaConverter
    {
        public string Id => typeof(Vector2Int).GetTypeId();
        public JsonNode GetScheme() => new JsonObject
        {
            [JsonSchema.Id] = Id,
            [JsonSchema.Type] = JsonSchema.Object,
            [JsonSchema.Properties] = new JsonObject
            {
                ["x"] = new JsonObject { [JsonSchema.Type] = JsonSchema.Number },
                ["y"] = new JsonObject { [JsonSchema.Type] = JsonSchema.Number }
            },
            [JsonSchema.Required] = new JsonArray { "x", "y" }
        };
        public JsonNode GetSchemeRef() => new JsonObject
        {
            [JsonSchema.Ref] = Id
        };

        public override Vector2Int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            int x = 0, y = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Vector2Int(x, y);

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "x":
                            x = reader.GetInt32();
                            break;
                        case "y":
                            y = reader.GetInt32();
                            break;
                        default:
                            throw new JsonException($"Unexpected property name: {propertyName}. "
                                + "Expected 'x' or 'y'.");
                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Vector2Int value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.x);
            writer.WriteNumber("y", value.y);
            writer.WriteEndObject();
        }
    }
}
