using System;
using AmongUsSpecimen.Utils;

namespace AmongUsSpecimen.Cosmetics;

[AttributeUsage(AttributeTargets.Class)]
public class CustomCosmeticsAttribute : Attribute
{
    public readonly string Repository;
    public readonly string ManifestFileName;
    public readonly string CustomDirectory;

    public CustomCosmeticsAttribute(string repository, string manifestFileName, string customDirectory = "")
    {
        AttributeHelpers.CheckRepositorySyntax(repository);
        Repository = repository;
        ManifestFileName = manifestFileName;
        CustomDirectory = customDirectory;
    }
}