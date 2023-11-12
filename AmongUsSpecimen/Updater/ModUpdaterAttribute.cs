using System;
using System.Collections.Generic;
using AmongUsSpecimen.Utils;

namespace AmongUsSpecimen.Updater;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ModUpdaterAttribute : Attribute
{
    public readonly string Directory;
    public readonly string RepositoryOwner;
    public readonly string RepositoryName;
    public readonly List<string> FilesToUpdate = new();
    public readonly Version VersionToCompare;
    public readonly Func<ModUpdaterConfig, GithubRelease, bool> CheckCompatibility;
    
    public ModUpdaterAttribute(string repository, string version, string file, string directory = "")
    {
        AttributeHelpers.CheckRepositorySyntax(repository);
        var rawRepository = repository.Split("/");
        var repositoryOwner = rawRepository[0];
        var repositoryName = rawRepository[1];
        CheckCompatibility = ModUpdaterManager.DefaultCompatibilityCheck;
        Directory = directory;
        RepositoryOwner = repositoryOwner;
        RepositoryName = repositoryName;
        VersionToCompare = Version.Parse(version);
        FilesToUpdate.Add(file);
    }
}