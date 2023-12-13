using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUsSpecimen.ModOptions;
using AmongUsSpecimen.Utils;
using UnityEngine;
using static AmongUsSpecimen.ModOptions.Helpers;

namespace AmongUsSpecimen;

[ModOptionContainer(ContainerType.Tabs)]
public static class OptionTabs
{
    public static readonly ModOptionTab MainTab;

    static OptionTabs()
    {
        MainTab = new ModOptionTab("Main", "Mods Settings", GetSprite("ModSettings"));
    }

    private static Sprite GetSprite(string name)
    {
        return Assembly.GetExecutingAssembly()
            .LoadSpriteFromResources($"AmongUsSpecimen.Resources.Sprites.{name}TabIcon.png", 400f);
    }
}

[ModOptionContainer]
public static class CoreOptions
{
    public static readonly ModStringOption PresetSelection;

    static CoreOptions()
    {
        PresetSelection = Local(OptionTabs.MainTab.StringOption("Preset", GetPresetNames, GetPresetNames()[0]));
        PresetSelection.ValueChanged += UpdatePreset;
        PresetSelection.OnUiLabelClick = OnPresetLabelClick;
        GameEventManager.HostChanged += OnHostChanged;
    }

    private static void OnPresetLabelClick()
    {
        ModOptionManager.PresetManagerWindow?.Toggle();
    }

    private static void OnHostChanged()
    {
        PresetSelection.UiOption?.UiUpdate();
    }

    private static void UpdatePreset()
    {
        var presetIdx = PresetSelection.CurrentSelection;
        if (presetIdx < 0) return;
        OptionStorage.Current.CurrentPresetIdx = presetIdx;
        OptionStorage.Persist();
        OptionStorage.ApplyCurrentPreset();
    }

    private static readonly List<string> ManagedByHostPresetNames = ["#Host Only#"];

    public static List<string> GetPresetNames()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost) return ManagedByHostPresetNames;
        var list = new List<string> { OptionStorage.Current.OnlinePreset.Name };
        list.AddRange(OptionStorage.Current.Presets.Select(preset => preset.Name));
        return list;

    }
}