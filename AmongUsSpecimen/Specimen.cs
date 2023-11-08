using System.Collections.Generic;
using AmongUsSpecimen.Attributes;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.Updater;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UniverseLib;

namespace AmongUsSpecimen;

// ReSharper disable once ClassNeverInstantiated.Global
[BepInPlugin(Guid, Name, Version)]
[BepInProcess("Among Us")]
[ModUpdater("EnoPM", "Specimen", Version, "AmongUsSpecimen.dll", "Specimen")]
[ModUpdater("EnoPM", "UniverseLib", Universe.VERSION, "UniverseLib.IL2CPP.dll", "Specimen")]
public class Specimen : BasePlugin
{
    public const string Guid = "specimen.eno.pm";
    private const string Name = "AmongUsSpecimen";
    private const string Version = "0.0.1";
    
    internal static Specimen Instance { get; private set; }
    internal static readonly Harmony Harmony = new(Guid);
    internal static readonly List<AutoUpdatedMod> AutoUpdatedMods = new();
    internal static readonly List<UpdateModConfig> UpdateRequests = new();
    internal UpdaterBehaviour UpdaterBehaviour;
    
    public ConfigEntry<KeyCode> ToggleUpdater { get; private set; }
    
    public override void Load()
    {
        Instance = this;
        
        // Plugin startup logic
        ToggleUpdater = Config.Bind("Updater", "Toggle Updaters", KeyCode.F2);
        
        Harmony.PatchAll();
        UiManager.Init();
        UpdaterBehaviour = AddComponent<UpdaterBehaviour>();
        
        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    public static bool DefaultCompatibilityCheck(UpdateModConfig config, GithubRelease release)
    {
        var isVersionCompatible = release.Version.Major == config.VersionToCompare.Major && release.Version.Minor == config.VersionToCompare.Minor;
        return isVersionCompatible || !release.Description.Contains(Application.version);
    }

    public static void RegisterModUpdater(UpdateModConfig config)
    {
        UpdateRequests.Add(config);
    }

    [Rpc]
    internal static void TestRpc(PlayerControl __sender, string message, List<int> testList)
    {
        System.Console.WriteLine($"{__sender.Data.PlayerName} send {message} with a list of {testList.Count} elements");
    }


    internal static void SendTestRpc()
    {
        TestRpc(PlayerControl.LocalPlayer, "Hello", new List<int> { 1, 2, 3, 4, 5 });
    }
}