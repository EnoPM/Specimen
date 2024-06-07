using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AmongUs.GameOptions;

namespace AmongUsSpecimen.ModOptions;

internal static class OptionStorage
{
    private static string PresetConfigFile => Path.Combine(Specimen.ResourcesDirectory, "Presets.json");
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
            CurrentPresetIdx = 1,
            OnlinePreset = defaultOnlinePreset,
            Presets = [defaultCurrentPreset],
            Global = new Dictionary<int, int>(),
            Local = new Dictionary<int, int>()
        };
        Persist();
    }

    internal static void SavePreset(ModOptionPreset preset)
    {
        var allPresets = new List<ModOptionPreset> { Current.OnlinePreset };
        allPresets.AddRange(Current.Presets);
        var resource = allPresets.Find(x => x == preset);
        if (resource != null)
        {
            resource.Update(preset);
        }
        else
        {
            Current.Presets.Add(preset);
        }
        
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

    internal static void ApplyVanillaOptions()
    {
        var currentPreset = Current.GetCurrentPreset();
        if (GameOptionsManager.Instance == null || !GameManager.Instance || GameManager.Instance.LogicOptions == null || GameManager.Instance.LogicOptions.currentGameOptions == null || string.IsNullOrEmpty(currentPreset.VanillaOptions))
        {
            Specimen.Instance.Log.LogWarning($"Unable to apply vanilla options from current preset {currentPreset.Name}");
            return;
        }
        GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.gameOptionsFactory.FromBytes(Convert.FromBase64String(currentPreset.VanillaOptions));
        GameOptionsManager.Instance.CurrentGameOptions = GameOptionsManager.Instance.GameHostOptions;
        GameManager.Instance.LogicOptions.SetGameOptions(GameOptionsManager.Instance.CurrentGameOptions);
        GameManager.Instance.LogicOptions.SyncOptions();
    }

    internal static void SaveVanillaOptions()
    {
        Current.GetCurrentPreset().VanillaOptions = Convert.ToBase64String(GameOptionsManager.Instance.gameOptionsFactory.ToBytes(GameManager.Instance.LogicOptions.currentGameOptions, false));
        Persist();
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
        
        ApplyVanillaOptions();
    }
}