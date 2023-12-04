using System;
using System.Collections.Generic;

namespace AmongUsSpecimen.ModOptions;

public static class ModOptionTabExtensions
{
    public static ModBoolOption BoolOption(
        this ModOptionTab tab,
        string name,
        bool defaultValue,
        BaseModOption parent = null)
    {
        return new ModBoolOption(tab, name, defaultValue, parent);
    }
    
    public static ModBoolOption BoolOption(
        this ModOptionTab tab,
        string name,
        Func<List<string>> selectionsGetter,
        bool defaultValue,
        BaseModOption parent = null)
    {
        return new ModBoolOption(tab, name, selectionsGetter, defaultValue, parent);
    }

    public static ModFloatOption FloatOption(
        this ModOptionTab tab,
        string name,
        float minValue,
        float maxValue,
        float step,
        float defaultValue,
        BaseModOption parent = null,
        string prefix = "",
        string suffix = "")
    {
        return new ModFloatOption(tab, name, minValue, maxValue, step, defaultValue, parent, prefix, suffix);
    }
    
    public static ModFloatOption FloatOption(
        this ModOptionTab tab,
        string name,
        Func<List<string>> selectionsGetter,
        float defaultValue,
        BaseModOption parent = null,
        string prefix = "",
        string suffix = "")
    {
        return new ModFloatOption(tab, name, selectionsGetter, defaultValue, parent, prefix, suffix);
    }

    public static ModStringOption StringOption(
        this ModOptionTab tab,
        string name,
        List<string> selections,
        string defaultValue,
        BaseModOption parent = null)
    {
        return new ModStringOption(tab, name, selections, defaultValue, parent);
    }
    
    public static ModStringOption StringOption(
        this ModOptionTab tab,
        string name,
        Func<List<string>> selectionsGetter,
        string defaultValue,
        BaseModOption parent = null)
    {
        return new ModStringOption(tab, name, selectionsGetter, defaultValue, parent);
    }
}