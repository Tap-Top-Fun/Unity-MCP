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
    public class BoundsIntConverter : JsonConverter<BoundsInt>, IJsonSchemaConverter
    {
        public string Id => typeof(BoundsInt).GetTypeId();
        public JsonNode GetScheme() => new JsonObject
        {
            [JsonUtils.Schema.Id] = Id,
            [JsonUtils.Schema.Type] = JsonUtils.Schema.Object,
            [JsonUtils.Schema.Properties] = new JsonObject
            {
                ["position"] = new JsonObject
                {
                    [JsonUtils.Schema.Type] = JsonUtils.Schema.Object,
                    [JsonUtils.Schema.Properties] = new JsonObject
                    {
                        ["x"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer },
                        ["y"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer },
                        ["z"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer }
                    },
                    [JsonUtils.Schema.Required] = new JsonArray { "x", "y", "z" }
                },
                ["size"] = new JsonObject
                {
                    [JsonUtils.Schema.Type] = JsonUtils.Schema.Object,
                    [JsonUtils.Schema.Properties] = new JsonObject
                    {
                        ["x"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer },
                        ["y"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer },
                        ["z"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Integer }
                    },
                    [JsonUtils.Schema.Required] = new JsonArray { "x", "y", "z" }
                }
            },
            [JsonUtils.Schema.Required] = new JsonArray { "position", "size" }
        };
        public JsonNode GetSchemeRef() => new JsonObject
        {
            [JsonUtils.Schema.Ref] = Id
        };

        public override BoundsInt Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object token.");

            Vector3Int position = Vector3Int.zero;
            Vector3Int size = Vector3Int.zero;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new BoundsInt(position, size);

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "position":
                            position = ReadVector3Int(ref reader);
                            break;
                        case "size":
                            size = ReadVector3Int(ref reader);
                            break;
                        default:
                            throw new JsonException($"Unexpected property name: {propertyName}. "
                                + "Expected 'position' or 'size'.");
                    }
                }
            }

            throw new JsonException("Expected end of object token.");
        }

        public override void Write(Utf8JsonWriter writer, BoundsInt value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("position");
            WriteVector3Int(writer, value.position);

            writer.WritePropertyName("size");
            WriteVector3Int(writer, value.size);

            writer.WriteEndObject();
        }

        private Vector3Int ReadVector3Int(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object token for Vector3Int.");

            int x = 0, y = 0, z = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Vector3Int(x, y, z);

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
                        case "z":
                            z = reader.GetInt32();
                            break;
                        default:
                            throw new JsonException($"Unexpected property name: {propertyName}. "
                                + "Expected 'x', 'y', or 'z'.");
                    }
                }
            }

            throw new JsonException("Expected end of object token for Vector3Int.");
        }

        private void WriteVector3Int(Utf8JsonWriter writer, Vector3Int value)
        {
            writer.WriteStartObject();
            writer.WriteNumber("x", value.x);
            writer.WriteNumber("y", value.y);
            writer.WriteNumber("z", value.z);
            writer.WriteEndObject();
        }
    }
}
