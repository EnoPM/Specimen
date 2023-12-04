using System;

namespace AmongUsSpecimen.ModOptions;

[AttributeUsage(AttributeTargets.Class)]
public class ModOptionContainerAttribute(ContainerType containerType = ContainerType.Options) : Attribute
{
    internal readonly ContainerType ContainerType = containerType;
}

public enum ContainerType
{
    Options,
    Tabs,
}