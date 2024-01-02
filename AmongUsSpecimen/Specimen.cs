using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUsSpecimen.Cosmetics;
using AmongUsSpecimen.ModOptions;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.Updater;
using AmongUsSpecimen.Utils;
using AmongUsSpecimen.VersionCheck;
using BepInEx;
using BepInEx.Configuration;
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
    private const string Version = "0.1.2";

    internal const string ToggleSpecimenDashboardActionName = "ActionToggleSpecimenDashboard";

    internal static readonly List<string> EmbeddedLibraries = new() { "UniverseLib.IL2CPP" };

    static Specimen()
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelpers.EmbeddedAssemblyResolver;
    }

    internal static Specimen Instance { get; private set; }
    internal static readonly Harmony Harmony = new(Guid);

    internal static string AmongUsDirectory => Path.GetDirectoryName(Application.dataPath)!;
    internal static string ResourcesDirectory => Path.Combine(AmongUsDirectory, "Specimen");
    internal static ConfigEntry<bool> UseSpecimenRegionsManager { get; private set; }

    public override void Load()
    {
        Instance = this;
        
        // Plugin startup logic
        UseSpecimenRegionsManager = Config.Bind("Core", "Use Custom Regions Manager", true, "If set to false, Specimen will no override Among Us regions manager.");
        if (!Directory.Exists(ResourcesDirectory)) Directory.CreateDirectory(ResourcesDirectory);

        Harmony.PatchAll();
        UiManager.Initialized += UiManagerOnInitialized;
        UiManager.Init();

        IL2CPPChainloader.Instance.PluginLoad += PluginLoad;

        InitSpecimenInAssembly(Assembly.GetExecutingAssembly(), Guid, Name);

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private static void UiManagerOnInitialized()
    {
        NotificationManager.Start();
        ModManager.Instance.ShowModStamp();
        VersionHandshakeManager.Start();
        ModOptionManager.PresetManagerWindow = UiManager.RegisterWindow<PresetManagerWindow>();
    }

    private void PluginLoad(PluginInfo pluginInfo, Assembly assembly, BasePlugin plugin)
    {
        if (pluginInfo.Dependencies.All(x => x.DependencyGUID != Guid)) return;
        Log.LogMessage(
            $"Loaded plugin: {pluginInfo.Metadata.Name} v{pluginInfo.Metadata.Version} ({assembly.ManifestModule.ModuleVersionId})");
        InitSpecimenInAssembly(assembly, pluginInfo.Metadata.GUID, pluginInfo.Metadata.Name);
    }

    private static void InitSpecimenInAssembly(Assembly assembly, string guid, string name)
    {
        ModUpdaterManager.RegisterAssembly(assembly);
        CustomRegionsManager.RegisterAssembly(assembly);
        RpcManager.RegisterAssembly(assembly);
        CustomCosmeticsManager.RegisterAssembly(assembly);
        RegisterMonoBehaviourAttribute.RegisterAssembly(assembly);
        CustomKeyBindManager.RegisterAssembly(assembly);
        VersionHandshakeManager.RegisterAssembly(assembly);
        ModOptionManager.RegisterAssembly(assembly);
        
        Instance.Log.LogMessage($"LocalHandshake: {VersionHandshakeManager.LocalHandshake.Mods["AmongUsSpecimen"].Version}");
    }
}