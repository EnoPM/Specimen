using System;
using AmongUsSpecimen.Utils;

namespace AmongUsSpecimen.Cosmetics;

[AttributeUsage(AttributeTargets.Class)]
public class CustomCosmeticsAttribute : Attribute
{
    public readonly string Repository;
    public readonly string ManifestFileName;

    public CustomCosmeticsAttribute(string repository, string manifestFileName)
    {
        AttributeHelpers.CheckRepositorySyntax(repository);
        Repository = repository;
        ManifestFileName = manifestFileName;
    }
}