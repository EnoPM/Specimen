using System;
using System.Collections.Generic;

namespace AmongUsSpecimen.Updater;

public class ModUpdaterConfig
{
    public string Directory;
    public string RepositoryOwner;
    public string RepositoryName;
    public readonly List<string> FilesToUpdate = new();
    public Version VersionToCompare;
    public Func<ModUpdaterConfig, GithubRelease, bool> CheckCompatibility = ModUpdaterManager.DefaultCompatibilityCheck;

    public ModUpdaterConfig()
    {
        
    }

    internal ModUpdaterConfig(ModUpdaterAttribute attribute)
    {
        Directory = attribute.Directory;
        RepositoryOwner = attribute.RepositoryOwner;
        RepositoryName = attribute.RepositoryName;
        FilesToUpdate.AddRange(attribute.FilesToUpdate);
        VersionToCompare = attribute.VersionToCompare;
        CheckCompatibility = attribute.CheckCompatibility;
    }
}