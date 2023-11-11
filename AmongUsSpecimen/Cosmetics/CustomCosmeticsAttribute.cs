using System;

namespace AmongUsSpecimen.Cosmetics;

[AttributeUsage(AttributeTargets.Class)]
public class CustomCosmeticsAttribute : Attribute
{
    public readonly string Repository;
    public readonly string ManifestFileName;

    public CustomCosmeticsAttribute(string repository, string manifestFileName)
    {
        if (!repository.Contains('/'))
        {
            throw new Exception($"Wrong repository format. Accepted format: owner/repository");
        }

        Repository = repository;
        ManifestFileName = manifestFileName;
    }
}