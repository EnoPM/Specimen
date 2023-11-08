using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AmongUsSpecimen.Utils;

internal static class AssemblyExtensions
{
    public static List<AttributeHelpers.AttributeMethodResult<TAttribute>> GetMethodsByAttribute<TAttribute>(this Assembly assembly)
        where TAttribute : Attribute
    {
        var results = new List<AttributeHelpers.AttributeMethodResult<TAttribute>>();
        var allClasses = assembly.GetTypes().Where(x => x.IsClass);
        foreach (var type in allClasses)
        {
            var allMethods = type.GetMethods().Where(x => x.GetCustomAttributes<TAttribute>().ToArray().Length > 0);
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
        var allClasses = assembly.GetTypes()
            .Where(x => x.IsClass && x.GetCustomAttributes<TAttribute>().ToArray().Length > 0);
        foreach (var type in allClasses)
        {
            var attributes = type.GetCustomAttributes<TAttribute>();
            results.AddRange(attributes.Select(x => new AttributeHelpers.AttributeClassResult<TAttribute>(x, type)));
        }
        return results;
    }
}