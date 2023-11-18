using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmongUsSpecimen.VersionCheck;

public class VersionHandshake
{
    [JsonPropertyName("c")] public int ClientId { get; set; } = -1;
    [JsonPropertyName("m")] public Dictionary<string, ModVersion> Mods { get; set; }
    [JsonPropertyName("f")] public Dictionary<string, string> Flags { get; set; }

    public class ModVersion
    {
        [JsonPropertyName("v")] public Version Version { get; set; }

        [JsonPropertyName("g")] public Guid Guid { get; set; }

        [JsonPropertyName("c")] public bool CheckGuid { get; set; }
    }
}