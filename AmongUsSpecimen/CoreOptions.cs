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
    
    
    public static readonly ModBoolOption RandomMap;
    internal static readonly RandomMapModOptionMap TheSkeldMap;
    internal static readonly RandomMapModOptionMap PolusMap;
    internal static readonly RandomMapModOptionMap MiraHqMap;
    internal static readonly RandomMapModOptionMap AirshipMap;
    internal static readonly RandomMapModOptionMap TheFungleMap;

    static CoreOptions()
    {
        PresetSelection = Local(OptionTabs.MainTab.StringOption("Preset", GetPresetNames, GetPresetNames()[0]));
        PresetSelection.ValueChanged += UpdatePreset;
        GameEventManager.HostChanged += OnHostChanged;
        

        RandomMap = OutsidePreset(OptionTabs.MainTab.BoolOption("Random Map", false));

        TheSkeldMap = new RandomMapModOptionMap("The Skeld");
        PolusMap = new RandomMapModOptionMap("Polus");
        MiraHqMap = new RandomMapModOptionMap("Mira HQ");
        AirshipMap = new RandomMapModOptionMap("Airship");
        TheFungleMap = new RandomMapModOptionMap("The Fungle");
    }

    private static void OnHostChanged()
    {
        PresetSelection.UiOption?.UiUpdate();
        foreach (var mapOptions in RandomMapModOptionMap.AllMaps)
        {
            mapOptions.PresetName.UiOption?.UiUpdate();
        }
    }

    private static void UpdatePreset()
    {
        var presetName = PresetSelection.GetString();
        var presetIdx = OptionStorage.Current.Presets.FindIndex(x => x.Name == presetName);
        if (presetIdx < 0) return;
        OptionStorage.Current.CurrentPresetIdx = presetIdx;
        OptionStorage.Persist();
        OptionStorage.ApplyCurrentPreset();
    }

    private static readonly List<string> ManagedByHostPresetNames = ["#Host Only#"];

    private static List<string> GetPresetNames()
    {
        if (AmongUsClient.Instance && AmongUsClient.Instance.AmHost)
        {
            return OptionStorage.Current.Presets.Select(x => x.Name).ToList();
        }

        return ManagedByHostPresetNames;
    }
    
    internal class RandomMapModOptionMap
    {
        internal static readonly List<RandomMapModOptionMap> AllMaps = [];
        
        internal readonly ModFloatOption Percentage;
        internal readonly ModBoolOption ShouldUseSpecificPreset;
        internal readonly ModStringOption PresetName;

        internal RandomMapModOptionMap(string mapName)
        {
            Percentage = OutsidePreset(OptionTabs.MainTab.FloatOption(mapName, 0f, 100f, 1f, 0f, RandomMap, suffix: "%"));
            ShouldUseSpecificPreset = OutsidePreset(OptionTabs.MainTab.BoolOption("Specific Preset", false, Percentage));
            PresetName = OutsidePreset(OptionTabs.MainTab.StringOption("Map Preset", GetPresetNames, GetPresetNames()[0], ShouldUseSpecificPreset));
            AllMaps.Add(this);
        }
    }
}