using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmongUsSpecimen.Utils.Converters;

public class SystemGuidConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            if (!string.IsNullOrEmpty(value))
            {
                return Guid.Parse(value);
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}