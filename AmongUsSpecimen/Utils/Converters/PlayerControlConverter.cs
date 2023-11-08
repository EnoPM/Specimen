using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmongUsSpecimen.Utils.Converters;

public class PlayerControlConverter : JsonConverter<PlayerControl>
{
    public override PlayerControl Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetInt32();
            if (value < 0) return null;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == (byte)value) return player;
            }

            return null;
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, PlayerControl value, JsonSerializerOptions options)
    {
        if (value != null)
        {
            writer.WriteNumberValue(value.PlayerId);
            return;
        }
        writer.WriteNullValue();
    }
}