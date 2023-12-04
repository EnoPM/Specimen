using System.Text.Json.Serialization;

namespace AmongUsSpecimen.Cosmetics;

public class CustomHat : ICustomHat
{
    [JsonPropertyName("author")] public string Author { get; set; }

    [JsonPropertyName("bounce")] public bool Bounce { get; set; }

    [JsonPropertyName("climbresource")] public string ClimbResource { get; set; }
    [JsonPropertyName("reshashc")] public string ClimbResource_Hash { get; set; }

    [JsonPropertyName("condition")] public string Condition { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("package")] public string Package { get; set; }

    [JsonPropertyName("resource")] public string Resource { get; set; }
    [JsonPropertyName("reshasha")] public string Resource_Hash { get; set; }

    [JsonPropertyName("adaptive")] public bool Adaptive { get; set; }

    [JsonPropertyName("behind")] public bool Behind { get; set; }

    [JsonPropertyName("backresource")] public string BackResource { get; set; }
    [JsonPropertyName("reshashb")] public string BackResource_Hash { get; set; }

    [JsonPropertyName("backflipresource")] public string BackFlipResource { get; set; }
    [JsonPropertyName("reshashbf")] public string BackFlipResource_Hash { get; set; }

    [JsonPropertyName("flipresource")] public string FlipResource { get; set; }
    [JsonPropertyName("reshashf")] public string FlipResource_Hash { get; set; }
    
    internal string HatsDirectoryPath { get; set; }
}