using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AmongUsSpecimen.Utils;

public static class AttributeHelpers
{
    public static List<AttributeMethodResult<T>> GetMethodsByAttribute<T>(Assembly assembly) where T : Attribute
    {
        return GetMethodsByAttribute<T>(new List<Assembly> { assembly });
    }

    public static List<AttributeMethodResult<T>> GetMethodsByAttribute<T>(List<Assembly> assemblies) where T : Attribute
    {
        var results = new List<AttributeMethodResult<T>>();

        var allClass = assemblies.SelectMany(x => x.GetTypes()).Where(x => x is { IsClass: true });
        foreach (var aClass in allClass)
        {
            var allMethods = aClass.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes(typeof(T), false).FirstOrDefault() != null).ToList();
            foreach (var aMethod in allMethods)
            {
                var attributes = aMethod.GetCustomAttributes(typeof(T)).Select(attribute => (T)attribute);
                results.AddRange(
                    attributes.Select(attribute => new AttributeMethodResult<T>(attribute, aMethod, aClass)));
            }
        }

        return results;
    }

    public static List<AttributeClassResult<T>> GetClassesByAttribute<T>(Assembly assembly) where T : Attribute
    {
        return GetClassesByAttribute<T>(new List<Assembly> { assembly });
    }

    public static List<AttributeClassResult<T>> GetClassesByAttribute<T>(List<Assembly> assemblies) where T : Attribute
    {
        var results = new List<AttributeClassResult<T>>();
        var classes = assemblies.SelectMany(x => x.GetTypes())
            .Where(x => x is { IsClass: true });
        foreach (var classType in classes)
        {
            var attribute = (T)classType.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            if (attribute != null)
            {
                results.Add(new AttributeClassResult<T>(attribute, classType));
            }
        }

        return results;
    }

    public class AttributeClassResult<T> where T : Attribute
    {
        public T Attribute;
        public Type Type;

        public AttributeClassResult(T attribute, Type type)
        {
            Attribute = attribute;
            Type = type;
        }
    }

    public class AttributeMethodResult<T> where T : Attribute
    {
        public readonly T Attribute;
        public readonly MethodInfo Method;
        public readonly Type Declaring;

        public AttributeMethodResult(T attribute, MethodInfo method, Type declaring)
        {
            Attribute = attribute;
            Method = method;
            Declaring = declaring;
        }
    }
}