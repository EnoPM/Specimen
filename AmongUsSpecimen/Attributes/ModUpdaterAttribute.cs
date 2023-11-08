using System;
using AmongUsSpecimen.Updater;

namespace AmongUsSpecimen.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ModUpdaterAttribute : Attribute
{
    public ModUpdaterAttribute(string owner, string repository, string version, string file, string directory = "")
    {
        Specimen.RegisterModUpdater(new UpdateModConfig
        {
            CheckCompatibility = Specimen.DefaultCompatibilityCheck,
            Directory = directory,
            RepositoryOwner = owner,
            RepositoryName = repository,
            VersionToCompare = Version.Parse(version),
            FilesToUpdate = { file }
        });
    }
}