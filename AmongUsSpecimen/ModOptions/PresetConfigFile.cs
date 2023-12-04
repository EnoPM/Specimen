using System.Collections.Generic;

namespace AmongUsSpecimen.ModOptions;

internal class PresetConfigFile
{
    public int CurrentPresetIdx { get; set; }
    public ModOptionPreset OnlinePreset { get; set; }
    public List<ModOptionPreset> Presets { get; set; }
    
    public Dictionary<int, int> Global { get; set; }
    public Dictionary<int, int> Local { get; set; }

    public ModOptionPreset GetCurrentPreset() => Presets == null ? null : Presets.Count > CurrentPresetIdx ? Presets[CurrentPresetIdx] : Presets.Count > 0 ? Presets[0] : null;
}