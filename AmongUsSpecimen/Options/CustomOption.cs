using System;
using System.Collections.Generic;
using AmongUsSpecimen.Utils;
using UnityEngine;

namespace AmongUsSpecimen.Options;

public sealed class CustomOption
{
    public int Id => CustomOptionManager.Options.IndexOf(this);
    public Types Type;
    public string Name;
    public CustomOptionTab Tab;
    public List<string> AvailableSelections;
    public int DefaultSelection;
    public OptionBehaviour OptionBehaviour;
    public CustomOption Parent;
    public bool IsHeader => Parent == null;
    public string SelectionPrefix;
    public string SelectionSuffix;

    public const int MinSelection = 0;
    internal int MaxSelection => AvailableSelections.Count - 1;

    private int _currentSelection;
    public int CurrentSelection
    {
        get => _currentSelection;
        set
        {
            if (!PlayerConditions.AmHostOrNotInGame() || !SetCurrentSelection_Internal(value)) return;
            CustomOptionManager.RpcSetValue(Id, _currentSelection);
            if (OptionBehaviour != null)
            {
                if (Type == Types.Boolean && OptionBehaviour is ToggleOption boolOption)
                {
                    boolOption.oldValue = boolOption.CheckMark.enabled = _currentSelection > 0;
                }
                else if (Type != Types.Boolean && OptionBehaviour is StringOption stringOption)
                {
                    stringOption.oldValue = stringOption.Value = _currentSelection;
                    stringOption.ValueText.text = DisplayValue;
                }
            }
            ValueChanged?.Invoke();
        }
    }

    internal bool SetCurrentSelection_Internal(int value)
    {
        var oldSelection = _currentSelection;
        _currentSelection = value > MaxSelection ? MinSelection : value < MinSelection ? MaxSelection : value;
        return _currentSelection != oldSelection;
    }

    public string DisplayValue => Type == Types.Boolean
        ? CurrentSelection > 0 ? ColorHelpers.Colorize(Color.green, "yes") : ColorHelpers.Colorize(Color.red, "no")
        : $"{SelectionPrefix}{AvailableSelections[CurrentSelection]}{SelectionSuffix}";
    
    public string DisplayUiValue => Type == Types.Boolean
        ? CurrentSelection > 0 ? ColorHelpers.Colorize(Color.green, "\u2714") : ColorHelpers.Colorize(Color.red, "\u2716")
        : $"{SelectionPrefix}{AvailableSelections[CurrentSelection]}{SelectionSuffix}";

    public event Action ValueChanged;

    public CustomOption(CustomOptionTab tab, Types type, string name, List<string> availableSelections, int defaultSelection = 0,
        CustomOption parent = null, string prefix = "", string suffix = "")
    {
        Tab = tab;
        Type = type;
        Name = name;
        AvailableSelections = availableSelections;
        CurrentSelection = DefaultSelection = defaultSelection;
        Parent = parent;
        SelectionPrefix = prefix;
        SelectionSuffix = suffix;
        CustomOptionManager.RegisterOption(this);
    }
    
    public enum Types
    {
        Float,
        String,
        Boolean,
    }
}