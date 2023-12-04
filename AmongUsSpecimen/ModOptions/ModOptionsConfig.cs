using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AmongUsSpecimen.Utils.Converters;
// ReSharper disable ClassNeverInstantiated.Global

namespace AmongUsSpecimen.ModOptions;

public class ModOptionsConfig
{
    [JsonPropertyName("target")]
    public string Target { get; set; }
    
    [JsonPropertyName("version")]
    [JsonConverter(typeof(SystemVersionConverter))]
    public Version Version { get; set; }
    
    [JsonPropertyName("tabs")]
    public List<ModOptionsConfigTab> Tabs { get; set; }
    
    [JsonPropertyName("options")]
    public List<ModOptionsConfigOption> Options { get; set; }
}

public class ModOptionsConfigTab
{
    [JsonPropertyName("key")]
    public string Key { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("icon")]
    public ModOptionsConfigTabIcon Icon { get; set; }
}

public class ModOptionsConfigTabIcon
{
    public string Resource { get; set; }
}

public class ModOptionsConfigOption
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("type")]
    [JsonConverter(typeof(ModOptionTypeConverter))]
    public OptionType Type { get; set; }
    
    [JsonPropertyName("isLocal")]
    public bool IsLocal { get; set; }
    
    [JsonPropertyName("selections")]
    public ModOptionsConfigSelections Selections { get; set; }
    
    [JsonPropertyName("children")]
    public List<ModOptionsConfigOption> Children { get; set; }
    
    [JsonPropertyName("defaultValue")]
    public int DefaultValue { get; set; }
}

public class ModOptionsConfigSelections
{
    [JsonPropertyName("function")]
    public string Function { get; set; }
    
    [JsonPropertyName("range")]
    public IModOptionsConfigSelectionsRange Range { get; set; }
    
    [JsonPropertyName("value")]
    public List<string> Value { get; set; }
}

public class IModOptionsConfigSelectionsRange
{
    [JsonPropertyName("min")]
    public float Min { get; set; }
    
    [JsonPropertyName("max")]
    public float Max { get; set; }
    
    [JsonPropertyName("step")]
    public float Step { get; set; }
}