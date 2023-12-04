using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AmongUsSpecimen.ModOptions;

internal static class OptionStorage
{
    internal static string PresetConfigFile => Path.Combine(Specimen.ResourcesDirectory, "Presets.json");
    internal static readonly PresetConfigFile Current;

    static OptionStorage()
    {
        if (File.Exists(PresetConfigFile))
        {
            try
            {
                Current = JsonSerializer.Deserialize<PresetConfigFile>(File.ReadAllText(PresetConfigFile));
                return;
            }
            catch
            {
                File.Delete(PresetConfigFile);
            }
        }

        var defaultCurrentPreset = new ModOptionPreset
        {
            IsSharable = true,
            Name = "Default",
            Values = new Dictionary<int, int>()
        };

        var defaultOnlinePreset = new ModOptionPreset
        {
            IsSharable = true,
            Name = "Online",
            Values = new Dictionary<int, int>()
        };

        Current = new PresetConfigFile
        {
            CurrentPresetIdx = 0,
            OnlinePreset = defaultOnlinePreset,
            Presets = [defaultCurrentPreset, defaultOnlinePreset],
            Global = new Dictionary<int, int>(),
            Local = new Dictionary<int, int>()
        };
        Persist();
    }

    internal static void SavePreset(ModOptionPreset preset)
    {
        var resource = Current.Presets.Find(x => x.Name == preset.Name);
        resource.Update(preset);
        Persist();
    }

    internal static void SaveCurrentPreset() => SavePreset(Current.GetCurrentPreset());

    internal static void Persist()
    {
        File.WriteAllText(PresetConfigFile, JsonSerializer.Serialize(Current));
    }

    private static void Update(this ModOptionPreset from, ModOptionPreset to)
    {
        from.Name = to.Name;
        from.IsSharable = to.IsSharable;
        from.Values = to.Values;
    }

    internal static void ApplyCurrentPreset()
    {
        foreach (var option in ModOptionManager.Options)
        {
            int selection;
            switch (option.SaveLocation)
            {
                case OptionSaveLocation.Global:
                    if (!Current.Local.TryGetValue(option.Id, out selection)) continue;
                    break;
                case OptionSaveLocation.Local:
                    if (!Current.Local.TryGetValue(option.Id, out selection)) continue;
                    break;
                case OptionSaveLocation.Preset:
                    if (!Current.GetCurrentPreset().Values.TryGetValue(option.Id, out selection)) continue;
                    break;
                default:
                    continue;
            }

            if (selection != option.CurrentSelection)
            {
                option.SetCurrentSelection(selection, false);
            }
        }
    }
}