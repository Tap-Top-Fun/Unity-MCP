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
    public class RectIntConverter : JsonConverter<RectInt>, IJsonSchemaConverter
    {
        public string Id => typeof(RectInt).GetTypeId();
        public JsonNode GetScheme() => new JsonObject
        {
            [JsonUtils.Schema.Id] = Id,
            [JsonUtils.Schema.Type] = JsonUtils.Schema.Object,
            [JsonUtils.Schema.Properties] = new JsonObject
            {
                ["x"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer },
                ["y"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer },
                ["width"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer },
                ["height"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer }
            },
            [JsonUtils.Schema.Required] = new JsonArray { "x", "y", "width", "height" }
        };
        public JsonNode GetSchemeRef() => new JsonObject
        {
            [JsonUtils.Schema.Ref] = Id
        };

        public override RectInt Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object token.");

            int x = 0, y = 0, width = 0, height = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new RectInt(x, y, width, height);

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
                        case "width":
                            width = reader.GetInt32();
                            break;
                        case "height":
                            height = reader.GetInt32();
                            break;
                        default:
                            throw new JsonException($"Unexpected property name: {propertyName}. "
                                + "Expected 'x', 'y', 'width', or 'height'.");
                    }
                }
            }

            throw new JsonException("Expected end of object token.");
        }

        public override void Write(Utf8JsonWriter writer, RectInt value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.x);
            writer.WriteNumber("y", value.y);
            writer.WriteNumber("width", value.width);
            writer.WriteNumber("height", value.height);
            writer.WriteEndObject();
        }
    }
}
