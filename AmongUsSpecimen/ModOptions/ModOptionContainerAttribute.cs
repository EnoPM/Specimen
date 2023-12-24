using System;

namespace AmongUsSpecimen.ModOptions;

[AttributeUsage(AttributeTargets.Class)]
public class ModOptionContainerAttribute : Attribute
{
    internal readonly ContainerType ContainerType;
    public ModOptionContainerAttribute(ContainerType containerType = ContainerType.Options)
    {
        ContainerType = containerType;
    }
}

public enum ContainerType
{
    Options,
    Tabs,
}