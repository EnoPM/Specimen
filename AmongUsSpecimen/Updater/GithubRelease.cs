using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmongUsSpecimen.Updater;

public class GithubRelease
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("tag_name")]
    public string Tag { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("draft")]
    public bool Draft { get; set; }
    
    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; set; }
    
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }
    
    [JsonPropertyName("published_at")]
    public string PublishedAt { get; set; }
    
    [JsonPropertyName("body")]
    public string Description { get; set; }
    
    [JsonPropertyName("assets")]
    public List<GithubAsset> Assets { get; set; }

    public Version Version => Version.Parse(Tag.Replace("v", string.Empty));

    public bool IsNewer(Version version)
    {
        return Version > version;
    }
}