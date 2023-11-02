using System;
using System.Collections.Generic;

namespace AmongUsSpecimen.Utils;

public static class Singletons
{
    private static readonly Dictionary<Type, object> Instances = new();

    public static T Register<T>(T instance)
    {
        if (instance == null)
        {
            throw new Exception("Cannot register singleton with null value in Singletons");
        }

        if (Instances.ContainsKey(typeof(T)))
        {
            throw new Exception($"Cannot have multiple instance of {typeof(T).FullName} in Singletons");
        }
        Instances[typeof(T)] = instance;

        return instance;
    }

    public static void Remove<T>()
    {
        Remove(typeof(T));
    }

    public static void Remove(Type type)
    {
        Instances.Remove(type);
    }

    public static T Get<T>()
    {
        return (T)Get(typeof(T));
    }

    public static object Get(Type type)
    {
        if (Instances.TryGetValue(type, out var instance)) return instance;
        throw new Exception($"Cannot find registered singleton of type {type.FullName}");
    }

    public static bool Exists<T>()
    {
        return Exists(typeof(T));
    }

    public static bool Exists(Type type)
    {
        return Instances.ContainsKey(type);
    }
}