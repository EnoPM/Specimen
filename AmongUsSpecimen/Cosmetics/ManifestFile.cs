using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmongUsSpecimen.Cosmetics;

public class ManifestFile
{
    [JsonPropertyName("hats")] public List<CustomHat> Hats { get; set; }
}