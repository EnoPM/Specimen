using System;
using System.Collections.Generic;

namespace AmongUsSpecimen.Updater;

public class UpdateModConfig
{
    public readonly string Directory;
    public readonly string RepositoryOwner;
    public readonly string RepositoryName;
    public readonly List<string> FilesToUpdate;
    public readonly Version VersionToCompare;
}