using System;

namespace AmongUsSpecimen.VersionCheck;

[AttributeUsage(AttributeTargets.Class)]
public class VersionHandshakeAttribute : Attribute
{
    public readonly string Name;
    public readonly string Version;
    public readonly bool CheckGuid;

    public VersionHandshakeAttribute(string name, string version, bool checkGuid = true)
    {
        Name = name;
        Version = version;
        CheckGuid = checkGuid;
    }
}