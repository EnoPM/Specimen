using System;
using System.Collections.Generic;

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
}