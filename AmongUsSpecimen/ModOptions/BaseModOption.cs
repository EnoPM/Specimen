using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using AmongUsSpecimen.UI.LobbyHud;
using AmongUsSpecimen.Utils;
using UnityEngine;
using UniverseLib.UI;

namespace AmongUsSpecimen.ModOptions;

public abstract class BaseModOption
{
    public int Id { get; }
    public OptionType Type { get; }
    public ModOptionTab Tab { get; private init; }
    public string Name { get; }
    protected SelectionsController AvailableSelections { get; init; }
    public int DefaultSelection { get; protected init; }
    protected int _currentSelection { get; set; }

    public OptionRestriction Restriction { get; private set; } = OptionRestriction.Public;
    public OptionSaveLocation SaveLocation { get; private set; } = OptionSaveLocation.Preset;
    public bool EnabledIfParentDisabled { get; private set; }
    internal Action OnUiLabelClick { get; set; }

    public void Update()
    {
        try
        {
            UiOption?.UiUpdate();
            BehaviourUpdate();
        }
        catch
        {
        }
    }

    public BaseModOption SetRestriction(OptionRestriction restriction = OptionRestriction.Public)
    {
        Restriction = restriction;
        Update();
        return this;
    }

    public BaseModOption SetSaveLocation(OptionSaveLocation location = OptionSaveLocation.Preset)
    {
        SaveLocation = location;
        SetCurrentSelection(GetStorageSelection(), false);
        Update();
        return this;
    }

    public BaseModOption SetEnabledIfParentDisabled(bool value = false)
    {
        EnabledIfParentDisabled = value;
        Update();
        return this;
    }

    public BaseModOption SetIsHeader(bool? value = null)
    {
        if (!value.HasValue)
        {
            IsHeader = Parent == null;
        }
        else
        {
            IsHeader = value.Value;
        }

        Update();
        return this;
    }

    public int CurrentSelection
    {
        get => _currentSelection;
        set => SetCurrentSelection(value);
    }

    internal const float BaseOptionYOffset = 0.5f;
    internal const float AdditionalHeaderYOffset = 0.25f;

    internal void BehaviourUpdate()
    {
        var toReturn = 0f;
        if (!OptionBehaviour) return;
        if (Type == OptionType.Boolean)
        {
            var toggleOption = OptionBehaviour as ToggleOption;
            if (!toggleOption) return;
            toggleOption.CheckMark.enabled = toggleOption.oldValue = GetBool();
        }
        else
        {
            var stringOption = OptionBehaviour as StringOption;
            if (!stringOption) return;
            stringOption.Value = stringOption.oldValue = CurrentSelection;
            stringOption.ValueText.text = DisplayValue;
        }

        var enabled = IsParentEnabled;
        OptionBehaviour.gameObject.SetActive(enabled);
    }

    internal void SetCurrentSelection(int value, bool updatePreset = true)
    {
        var min = AvailableSelections.MinSelection;
        var max = AvailableSelections.MaxSelection;
        var index = value < min ? max : value > max ? min : value;
        if (index == _currentSelection) return;
        _currentSelection = index;
        if (Restriction == OptionRestriction.Public && SaveLocation != OptionSaveLocation.Local &&
            AmongUsClient.Instance && AmongUsClient.Instance.AmHost)
        {
            ModOptionManager.RpcSetOptionSelection(Id, _currentSelection);
        }

        BehaviourUpdate();

        if (updatePreset)
        {
            SaveOption();
        }

        ValueChanged?.Invoke();
    }

    public BaseModOption Parent { get; }
    internal OptionBehaviour OptionBehaviour { get; set; }
    internal UiCustomOption UiOption { get; set; }
    public bool IsHeader { get; private set; }
    internal bool IsEnabled => CurrentSelection > 0 && IsParentEnabled;

    internal bool IsParentEnabled => Parent == null || (!EnabledIfParentDisabled && Parent.IsEnabled) ||
                                     (EnabledIfParentDisabled && !Parent.IsEnabled && Parent.IsParentEnabled);

    internal IEnumerable<BaseModOption> Children => ModOptionManager.Options.Where(x => x.Parent == this);

    private string UniqueName => $"{Tab.Key}:" + Name + (Parent == null ? string.Empty : $":{Parent.UniqueName}");

    protected BaseModOption(OptionType type, ModOptionTab tab, string name, BaseModOption parent = null)
    {
        Tab = tab;
        Type = type;
        Name = name;
        Parent = parent;
        Id = GetInt32HashCode(UniqueName);
        ModOptionManager.Options.Add(this);
        IsHeader = Parent == null;
    }

    private void SaveOption()
    {
        switch (SaveLocation)
        {
            case OptionSaveLocation.Global:
                OptionStorage.Current.Global[Id] = CurrentSelection;
                break;
            case OptionSaveLocation.Local:
                OptionStorage.Current.Local[Id] = CurrentSelection;
                break;
            case OptionSaveLocation.Preset:
            default:
                OptionStorage.Current.GetCurrentPreset().Values[Id] = CurrentSelection;
                break;
        }

        OptionStorage.SaveCurrentPreset();
    }

    protected int GetStorageSelection()
    {
        switch (SaveLocation)
        {
            case OptionSaveLocation.Global:
                return OptionStorage.Current.Global.GetValueOrDefault(Id, DefaultSelection);
                break;
            case OptionSaveLocation.Local:
                return OptionStorage.Current.Local.GetValueOrDefault(Id, DefaultSelection);
                break;
            case OptionSaveLocation.Preset:
                return OptionStorage.Current.GetCurrentPreset().Values.GetValueOrDefault(Id, DefaultSelection);
            default:
                break;
        }

        return DefaultSelection;
    }

    private readonly SHA1 hash = SHA1.Create();

    private int GetInt32HashCode(string strText)
    {
        if (string.IsNullOrEmpty(strText)) return 0;
        var byteContents = Encoding.Unicode.GetBytes(strText);
        var hashText = hash.ComputeHash(byteContents);
        var hashCodeStart = BitConverter.ToInt32(hashText, 0);
        var hashCodeMedium = BitConverter.ToInt32(hashText, 8);
        var hashCodeEnd = BitConverter.ToInt32(hashText, 16);
        var hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
        return int.MaxValue - hashCode;
    }

    internal int GetUiIndentation()
    {
        var i = 0;
        var cursor = Parent;
        while (cursor != null)
        {
            if (cursor.Tab == Tab)
            {
                i++;
            }

            cursor = cursor.Parent;
        }

        return i;
    }

    internal string GetDisplayName(int prefixSize = 1)
    {
        const string blankChar = "H";
        var indentation = GetUiIndentation();
        if (indentation == 0) return Name;
        var prefix = string.Empty;
        for (var i = 0; i < indentation; i++)
        {
            prefix += blankChar;
        }

        return $"<size={prefixSize}>{ColorHelpers.Colorize(UIPalette.Transparent, prefix)}</size>{Name}";
    }

    public virtual string DisplayName => GetDisplayName();
    public virtual string DisplayValue => AvailableSelections.GetValue(CurrentSelection);
    public virtual string DisplayUiValue => AvailableSelections.GetValue(CurrentSelection);

    public event Action ValueChanged;

    public bool GetBool() => IsEnabled;
    public string GetString() => AvailableSelections.GetValue(CurrentSelection);
    public float GetFloat() => float.Parse(GetString());
    public int GetInt() => Mathf.RoundToInt(GetFloat());
    public static implicit operator bool(BaseModOption option) => option.GetBool();
    public static implicit operator string(BaseModOption option) => option.GetString();
    public static implicit operator float(BaseModOption option) => option.GetFloat();
    public static implicit operator int(BaseModOption option) => option.GetInt();
}