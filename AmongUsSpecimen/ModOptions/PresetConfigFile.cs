using System.Collections.Generic;

namespace AmongUsSpecimen.ModOptions;

internal class PresetConfigFile
{
    public int CurrentPresetIdx { get; set; }
    public ModOptionPreset OnlinePreset { get; set; }
    public List<ModOptionPreset> Presets { get; set; }
    
    public Dictionary<int, int> Global { get; set; }
    public Dictionary<int, int> Local { get; set; }

    public ModOptionPreset GetCurrentPreset()
    {
        if (CurrentPresetIdx == 0 || Presets == null || Presets.Count == 0) return OnlinePreset;
        return Presets.Count > CurrentPresetIdx - 1 ? Presets[CurrentPresetIdx - 1] : Presets[0];
    }
}