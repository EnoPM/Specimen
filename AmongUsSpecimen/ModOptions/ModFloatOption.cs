using System;
using System.Collections.Generic;

namespace AmongUsSpecimen.ModOptions;

public class ModFloatOption : BaseModOption
{
    private readonly string _prefix;
    private readonly string _suffix;
    
    public override string DisplayValue => $"{_prefix}{AvailableSelections.GetValue(CurrentSelection)}{_suffix}";
    public override string DisplayUiValue => $"{_prefix}{AvailableSelections.GetValue(CurrentSelection)}{_suffix}";
    
    public ModFloatOption(ModOptionTab tab, string name, float minValue, float maxValue, float step, float defaultValue, BaseModOption parent = null, string prefix = "", string suffix = "") : base(OptionType.Float, tab, name, parent)
    {
        var selections = new List<string>();
        for (var i = minValue; i <= maxValue; i += step)
        {
            selections.Add($"{i}");
        }

        _prefix = prefix;
        _suffix = suffix;

        AvailableSelections = new SelectionsController(selections);
        DefaultSelection = AvailableSelections.Values.IndexOf($"{defaultValue}");
        _currentSelection = GetStorageSelection();
    }
    
    public ModFloatOption(ModOptionTab tab, string name, Func<List<string>> selectionsGetter, float defaultValue, BaseModOption parent = null, string prefix = "", string suffix = "") : base(OptionType.Float, tab, name, parent)
    {
        _prefix = prefix;
        _suffix = suffix;

        AvailableSelections = new SelectionsController(selectionsGetter);
        DefaultSelection = AvailableSelections.Values.IndexOf($"{defaultValue}");
        _currentSelection = GetStorageSelection();
    }
}