using System.Collections.Generic;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.Updater;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace AmongUsSpecimen;

// ReSharper disable once ClassNeverInstantiated.Global
[BepInPlugin(Guid, Name, Version)]
[BepInProcess("Among Us")]
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
    
    public override void Load()
    {
        Instance = this;
        
        // Plugin startup logic
        Harmony.PatchAll();
        UiManager.Init();
        UpdaterBehaviour = AddComponent<UpdaterBehaviour>();
        
        Log.LogInfo($"Plugin {Name} is loaded!");
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