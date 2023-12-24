using System.Collections.Generic;
using System.Reflection;
using AmongUsSpecimen.Utils;
using UnityEngine;

namespace AmongUsSpecimen.Updater;

public static class ModUpdaterManager
{
    internal static readonly List<AutoUpdatedMod> AutoUpdatedMods = new();
    internal static readonly List<ModUpdaterConfig> UpdateRequests = new();

    internal static readonly UpdaterBehaviour UpdaterBehaviour;

    static ModUpdaterManager()
    {
        UpdaterBehaviour = Specimen.Instance.AddComponent<UpdaterBehaviour>();
    }
    
    public static void RegisterModUpdater(ModUpdaterConfig updaterConfig)
    {
        UpdateRequests.Add(updaterConfig);
    }
    
    public static bool DefaultCompatibilityCheck(ModUpdaterConfig updaterConfig, GithubRelease release)
    {
        return !release.Description.Contains(Application.version);
    }

    internal static void RegisterAssembly(Assembly assembly)
    {
        var results = assembly.GetClassesByAttribute<ModUpdaterAttribute>();
        foreach (var result in results)
        {
            UpdateRequests.Add(new ModUpdaterConfig(result.Attribute));
        }
    }
}