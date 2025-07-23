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
    public class Color32Converter : JsonConverter<Color32>, IJsonSchemaConverter
    {
        public string Id => typeof(Color32).GetTypeId();
        public JsonNode GetScheme() => new JsonObject
        {
            [JsonUtils.Schema.Id] = Id,
            [JsonUtils.Schema.Type] = JsonUtils.Schema.Object,
            [JsonUtils.Schema.Properties] = new JsonObject
            {
                ["r"] = new JsonObject
                {
                    [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer,
                    [JsonUtils.Schema.Minimum] = 0,
                    [JsonUtils.Schema.Maximum] = 255
                },
                ["g"] = new JsonObject
                {
                    [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer,
                    [JsonUtils.Schema.Minimum] = 0,
                    [JsonUtils.Schema.Maximum] = 255
                },
                ["b"] = new JsonObject
                {
                    [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer,
                    [JsonUtils.Schema.Minimum] = 0,
                    [JsonUtils.Schema.Maximum] = 255
                },
                ["a"] = new JsonObject
                {
                    [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer,
                    [JsonUtils.Schema.Minimum] = 0,
                    [JsonUtils.Schema.Maximum] = 255
                }
            },
            [JsonUtils.Schema.Required] = new JsonArray { "r", "g", "b", "a" }
        };
        public JsonNode GetSchemeRef() => new JsonObject
        {
            [JsonUtils.Schema.Ref] = Id
        };

        public override Color32 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            byte r = 0, g = 0, b = 0, a = 255;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Color32(r, g, b, a);

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "r":
                            r = reader.GetByte();
                            break;
                        case "g":
                            g = reader.GetByte();
                            break;
                        case "b":
                            b = reader.GetByte();
                            break;
                        case "a":
                            a = reader.GetByte();
                            break;
                        default:
                            throw new JsonException($"Unexpected property name: {propertyName}. "
                                + $"Expected 'r', 'g', 'b', or 'a'.");
                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Color32 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("r", value.r);
            writer.WriteNumber("g", value.g);
            writer.WriteNumber("b", value.b);
            writer.WriteNumber("a", value.a);
            writer.WriteEndObject();
        }
    }
}
