using System;
using System.Collections.Generic;

namespace AmongUsSpecimen.ModOptions;

public class ModStringOption : BaseModOption
{
    public ModStringOption(ModOptionTab tab, string name, List<string> selections, string defaultValue, BaseModOption parent = null) : base(OptionType.String, tab, name, parent)
    {
        AvailableSelections = new SelectionsController(selections);
        DefaultSelection = AvailableSelections.Values.IndexOf(defaultValue);
        _currentSelection = GetStorageSelection();
    }
    
    public ModStringOption(ModOptionTab tab, string name, Func<List<string>> selectionsGetter, string defaultValue, BaseModOption parent = null) : base(OptionType.String, tab, name, parent)
    {
        AvailableSelections = new SelectionsController(selectionsGetter);
        DefaultSelection = AvailableSelections.Values.IndexOf(defaultValue);
    }
}