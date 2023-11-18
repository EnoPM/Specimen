using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using AmongUsSpecimen.Cosmetics;
using AmongUsSpecimen.Options;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.Updater;
using AmongUsSpecimen.Utils;
using AmongUsSpecimen.VersionCheck;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Rewired;
using UnityEngine;

namespace AmongUsSpecimen;

// ReSharper disable once ClassNeverInstantiated.Global
[BepInPlugin(Guid, Name, Version)]
[BepInProcess("Among Us")]
[ModUpdater("EnoPM/Specimen", Version, "AmongUsSpecimen.dll", "Specimen")]
[CustomKeyBind(ToggleSpecimenDashboardActionName, "Toggle Specimen HUD", KeyboardKeyCode.F10)]
[VersionHandshake(Name, Version)]
public class Specimen : BasePlugin
{
    public const string Guid = "specimen.eno.pm";
    private const string Name = "AmongUsSpecimen";
    private const string Version = "0.0.2";

    internal const string ToggleSpecimenDashboardActionName = "ActionToggleSpecimenDashboard";

    internal static readonly List<string> EmbeddedLibraries = new() { "UniverseLib.IL2CPP" };

    static Specimen()
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelpers.EmbeddedAssemblyResolver;
    }

    internal static Specimen Instance { get; private set; }
    internal static readonly Harmony Harmony = new(Guid);

    internal static string ResourcesDirectory => Path.Combine(Path.GetDirectoryName(Application.dataPath)!, "Specimen");

    public override void Load()
    {
        Instance = this;
        
        // Plugin startup logic
        if (!Directory.Exists(ResourcesDirectory)) Directory.CreateDirectory(ResourcesDirectory);

        Harmony.PatchAll();
        UiManager.Initialized += UiManagerOnInitialized;
        UiManager.Init();

        IL2CPPChainloader.Instance.PluginLoad += PluginLoad;

        InitSpecimenInAssembly(Assembly.GetExecutingAssembly());

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private static void UiManagerOnInitialized()
    {
        NotificationManager.Start();
        ModManager.Instance.ShowModStamp();
        VersionHandshakeManager.Start();
        var tab = new CustomOptionTab
        {
            Key = "TestTab",
            Title = "Test Tab Options",
            IconSprite = SpecimenSprites.ModSettingsTabIcon
        };
        CustomOptionManager.Tabs.Add(tab);
        var opt = new CustomOption(tab, CustomOption.Types.Boolean, "Test Option Boolean",
            new List<string> { "no", "yes" }, 0);
        var opt2 = new CustomOption(tab, CustomOption.Types.Float, "Test Option Float",
            new List<string> { "10", "12.5", "15", "17.5", "20", "22.5", "25", "27.5", "30", "32.5", "35", "37.5", "40" }, 0, opt, suffix: "s");
#if DEBUG
        NotificationManager.DemoNotification();
#endif
    }

    private void PluginLoad(PluginInfo pluginInfo, Assembly assembly, BasePlugin plugin)
    {
        if (pluginInfo.Dependencies.All(x => x.DependencyGUID != Guid)) return;
        Log.LogMessage(
            $"Loaded plugin: {pluginInfo.Metadata.Name} v{pluginInfo.Metadata.Version} ({assembly.ManifestModule.ModuleVersionId})");
        InitSpecimenInAssembly(assembly);
    }

    private static void InitSpecimenInAssembly(Assembly assembly)
    {
        ModUpdaterManager.RegisterAssembly(assembly);
        CustomRegionsManager.RegisterAssembly(assembly);
        RpcManager.RegisterAssembly(assembly);
        CustomCosmeticsManager.RegisterAssembly(assembly);
        RegisterInIl2CppAttribute.RegisterAssembly(assembly);
        CustomKeyBindManager.RegisterAssembly(assembly);
        VersionHandshakeManager.RegisterAssembly(assembly);
        
        Instance.Log.LogMessage($"LocalHandshake: {VersionHandshakeManager.LocalHandshake.Mods["AmongUsSpecimen"].Version}");
    }
}