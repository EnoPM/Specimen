using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUsSpecimen.Cosmetics;
using AmongUsSpecimen.Updater;
using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace AmongUsSpecimen.Utils;

internal static class AssemblyExtensions
{
    public static List<AttributeHelpers.AttributeMethodResult<TAttribute>> GetMethodsByAttribute<TAttribute>(this Assembly assembly)
        where TAttribute : Attribute
    {
        var results = new List<AttributeHelpers.AttributeMethodResult<TAttribute>>();
        var allClasses = assembly.GetExportedTypes().Where(x => x.IsClass);
        foreach (var type in allClasses)
        {
            var allMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(x => x.GetCustomAttributes<TAttribute>().ToArray().Length > 0);
            foreach (var method in allMethods)
            {
                var attributes = method.GetCustomAttributes<TAttribute>();
                results.AddRange(attributes.Select(x => new AttributeHelpers.AttributeMethodResult<TAttribute>(x, method, type)));
            }
        }

        return results;
    }

    public static List<AttributeHelpers.AttributeClassResult<TAttribute>> GetClassesByAttribute<TAttribute>(
        this Assembly assembly) where TAttribute : Attribute
    {
        var results = new List<AttributeHelpers.AttributeClassResult<TAttribute>>();
        var allClasses = assembly.GetExportedTypes()
            .Where(x => x.IsClass && x.GetCustomAttributes<TAttribute>().ToArray().Length > 0);
        foreach (var type in allClasses)
        {
            var attributes = type.GetCustomAttributes<TAttribute>();
            results.AddRange(attributes.Select(x => new AttributeHelpers.AttributeClassResult<TAttribute>(x, type)));
        }
        return results;
    }

    public static Assembly LoadEmbeddedLibrary(this Assembly assembly, string libraryName)
    {
        var key = $"{nameof(AmongUsSpecimen)}.Resources.Library.{libraryName}.dll";
        using var stream = assembly.GetManifestResourceStream(key);
        if (stream == null)
        {
            System.Console.WriteLine($"Unable to load library {key.Replace(".", "/")}");
            return null;
        }
        var data = new byte[stream.Length];
        _ = stream.Read(data, 0, data.Length);
        var result = Assembly.Load(data);
        System.Console.WriteLine($"Loaded embedded {libraryName}.dll: {result.ManifestModule.ModuleVersionId}");
        return result;
    }
}