using System;
using System.Collections.Generic;
using System.Linq;

namespace AmongUsSpecimen.Extensions;

public static class ListExtensions
{
    private static readonly Random Random = new();
    
    public static List<T> PickRandom<T>(this List<T> list, int count = 1)
    {
        var picked = 0;
        var pickedItems = new List<T>();
        if (count > list.Count) return pickedItems;
        while (picked < count)
        {
            pickedItems.Add(list.PickOneRandom());
            picked++;
        }

        return pickedItems;
    }

    public static T PickOneRandom<T>(this List<T> list)
    {
        var toRemove = list[Random.Next(0, list.Count)];
        list.Remove(toRemove);
        return toRemove;
    }

    public static T GetOneRandom<T>(this List<T> list)
    {
        return list[Random.Next(0, list.Count)];
    }

    public static void Replace<T>(this List<T> list, T toRemove, T toSet)
    {
        var index = list.IndexOf(toRemove);
        if (index < 0)
        {
            list.Add(toSet);
        }
        else
        {
            list[index] = toSet;
        }
    }

    public static List<T> Deduplicate<T>(this List<T> list, Func<T, T, bool> comparator)
    {
        var cache = new List<T>(list);
        var toRemove = new List<T>();
        foreach (var item in cache)
        {
            if (toRemove.Contains(item)) continue;
            var duplicates = cache.Where(x => comparator(x, item)).ToList();
            if (duplicates.Count > 1)
            {
                duplicates.Remove(item);
                toRemove.AddRange(duplicates);
            }
        }

        foreach (var item in toRemove)
        {
            cache.Remove(item);
        }
        
        return cache;
    }
}