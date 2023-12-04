using System;
using System.Collections.Generic;
using AmongUsSpecimen.Utils;
using UnityEngine;

namespace AmongUsSpecimen.ModOptions;

public class ModBoolOption : BaseModOption
{
    public override string DisplayValue => CurrentSelection > 0 ? ColorHelpers.Colorize(Color.green, "yes") : ColorHelpers.Colorize(Color.red, "no");
    public override string DisplayUiValue => CurrentSelection > 0 ? ColorHelpers.Colorize(Color.green, "\u2714") : ColorHelpers.Colorize(Color.red, "\u2716");
    
    public ModBoolOption(ModOptionTab tab, string name, bool defaultValue, BaseModOption parent = null) : base(OptionType.Boolean, tab, name, parent)
    {
        AvailableSelections = new SelectionsController([ColorHelpers.Colorize(Color.red, "no"), ColorHelpers.Colorize(Color.green, "yes")]);
        DefaultSelection = defaultValue ? 1 : 0;
        _currentSelection = GetStorageSelection();
    }
    
    public ModBoolOption(ModOptionTab tab, string name, Func<List<string>> selectionsGetter, bool defaultValue, BaseModOption parent = null) : base(OptionType.Boolean, tab, name, parent)
    {
        AvailableSelections = new SelectionsController(selectionsGetter);
        DefaultSelection = defaultValue ? 1 : 0;
        _currentSelection = GetStorageSelection();
    }
}