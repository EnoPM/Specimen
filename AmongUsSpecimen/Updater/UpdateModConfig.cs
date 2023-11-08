using System;
using System.Collections.Generic;

namespace AmongUsSpecimen.Updater;

public class UpdateModConfig
{
    public string Directory;
    public string RepositoryOwner;
    public string RepositoryName;
    public readonly List<string> FilesToUpdate = new();
    public Version VersionToCompare;
    public Func<UpdateModConfig, GithubRelease, bool> CheckCompatibility = Specimen.DefaultCompatibilityCheck;
}