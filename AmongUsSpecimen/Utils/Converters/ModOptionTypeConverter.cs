using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using AmongUsSpecimen.ModOptions;

namespace AmongUsSpecimen.Utils.Converters;

internal class ModOptionTypeConverter : JsonConverter<OptionType>
{
    public override OptionType Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) throw new JsonException();
        var value = reader.GetString();
        switch (value?.ToLowerInvariant())
        {
            case "string":
                return OptionType.String;
            case "float":
            case "int":
            case "int32":
                return OptionType.Float;
            case "bool":
            case "boolean":
                return OptionType.Boolean;
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, OptionType value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case OptionType.String:
                writer.WriteStringValue("string");
                return;
            case OptionType.Float:
                writer.WriteStringValue("float");
                return;
            case OptionType.Boolean:
                writer.WriteStringValue("bool");
                return;
            default:
                throw new JsonException();
        }
    }
}