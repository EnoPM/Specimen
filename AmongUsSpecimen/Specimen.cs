using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.Updater;
using AmongUsSpecimen.Utils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace AmongUsSpecimen;

// ReSharper disable once ClassNeverInstantiated.Global
[BepInPlugin(Guid, Name, Version)]
[BepInProcess("Among Us")]
[ModUpdater("EnoPM", "Specimen", Version, "AmongUsSpecimen.dll", "Specimen")]
public class Specimen : BasePlugin
{
    public const string Guid = "specimen.eno.pm";
    private const string Name = "AmongUsSpecimen";
    private const string Version = "0.0.1";

    internal static readonly List<string> EmbeddedLibraries = new() { "UniverseLib.IL2CPP" };

    static Specimen()
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyHelpers.EmbeddedAssemblyResolver;
    }

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

        IL2CPPChainloader.Instance.PluginLoad += PluginLoad;
        Assembly.GetExecutingAssembly().InitSpecimen();

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private void PluginLoad(PluginInfo pluginInfo, Assembly assembly, BasePlugin plugin)
    {
        if (pluginInfo.Dependencies.All(x => x.DependencyGUID != Guid)) return;
        Log.LogMessage($"Loaded plugin: {pluginInfo.Metadata.Name} v{pluginInfo.Metadata.Version} ({assembly.ManifestModule.ModuleVersionId})");
        assembly.InitSpecimen();
    }

    public static bool DefaultCompatibilityCheck(UpdateModConfig config, GithubRelease release)
    {
        var isVersionCompatible = release.Version.Major == config.VersionToCompare.Major &&
                                  release.Version.Minor == config.VersionToCompare.Minor;
        return isVersionCompatible || !release.Description.Contains(Application.version);
    }

    public static void RegisterModUpdater(UpdateModConfig config)
    {
        UpdateRequests.Add(config);
    }
}