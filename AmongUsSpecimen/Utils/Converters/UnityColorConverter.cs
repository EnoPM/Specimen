using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace AmongUsSpecimen.Utils.Converters;

public class UnityColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return string.IsNullOrEmpty(value) ? new Color() : ColorHelpers.FromHex(value);
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return new Color();
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.a < 1f ? ColorHelpers.ToHex(value) : ColorHelpers.ToHex(value, false));
    }
}