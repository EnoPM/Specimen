using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AmongUsSpecimen.Cosmetics;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.Updater;
using AmongUsSpecimen.Utils;
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
    }
}