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
    public class Matrix4x4Converter : JsonConverter<Matrix4x4>, IJsonSchemaConverter
    {
        public string Id => typeof(Matrix4x4).GetTypeId();
        public JsonNode GetScheme() => new JsonObject
        {
            [JsonUtils.Schema.Id] = Id,
            [JsonUtils.Schema.Type] = JsonUtils.Schema.Object,
            [JsonUtils.Schema.Properties] = new JsonObject
            {
                ["m00"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m01"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m02"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m03"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m10"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m11"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m12"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m13"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m20"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m21"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m22"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m23"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m30"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m31"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m32"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number },
                ["m33"] = new JsonObject { [JsonUtils.Schema.Type] = JsonUtils.Schema.Number }
            },
            [JsonUtils.Schema.Required] = new JsonArray
            {
                "m00", "m01", "m02", "m03",
                "m10", "m11", "m12", "m13",
                "m20", "m21", "m22", "m23",
                "m30", "m31", "m32", "m33"
            }
        };
        public JsonNode GetSchemeRef() => new JsonObject
        {
            [JsonUtils.Schema.Ref] = Id
        };

        public override Matrix4x4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            float[] elements = new float[16];
            int index = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Matrix4x4(
                        new Vector4(elements[0], elements[1], elements[2], elements[3]),
                        new Vector4(elements[4], elements[5], elements[6], elements[7]),
                        new Vector4(elements[8], elements[9], elements[10], elements[11]),
                        new Vector4(elements[12], elements[13], elements[14], elements[15])
                    );

                if (reader.TokenType == JsonTokenType.Number)
                {
                    elements[index++] = reader.GetSingle();
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Matrix4x4 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    writer.WriteNumber($"m{i}{j}", value[i, j]);
                }
            }
            writer.WriteEndObject();
        }
    }
}
