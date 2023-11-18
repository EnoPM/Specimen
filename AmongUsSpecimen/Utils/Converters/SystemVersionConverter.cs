using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmongUsSpecimen.Utils.Converters;

public class SystemVersionConverter : JsonConverter<Version>
{
    public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (!string.IsNullOrEmpty(value))
            {
                return Version.Parse(value);
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}