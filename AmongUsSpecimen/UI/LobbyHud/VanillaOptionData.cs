using System;
using AmongUs.GameOptions;

namespace AmongUsSpecimen.UI.LobbyHud;

internal sealed class VanillaOptionData
{
    internal readonly StringNames StringName;
    private readonly Int32OptionNames? IntOptionName;
    private readonly FloatOptionNames? FloatOptionName;
    private readonly BoolOptionNames? BoolOptionName;
    private readonly Func<string> LabelGetter;
    private readonly Func<object> ValueGetter;
    private readonly string ValueSuffix;
    private readonly string ValuePrefix;

    internal VanillaOptionData(StringNames stringName, Func<string> valueGetter)
    {
        StringName = stringName;
        ValueGetter = valueGetter;
        IsBool = IsInt = IsFloat = false;
        IsString = true;
    }

    internal VanillaOptionData(StringNames stringName, BoolOptionNames optionName)
    {
        StringName = stringName;
        BoolOptionName = optionName;
        IsInt = IsFloat = IsString = false;
        IsBool = true;
    }

    internal VanillaOptionData(StringNames stringName, Func<bool> valueGetter)
    {
        StringName = stringName;
        ValueGetter = valueGetter != null ? () => valueGetter() : null;
        IsInt = IsFloat = IsString = false;
        IsBool = true;
    }

    internal VanillaOptionData(Func<string> labelGetter, Func<bool> valueGetter)
    {
        LabelGetter = labelGetter;
        ValueGetter = valueGetter != null ? () => valueGetter() : null;
        IsInt = IsFloat = IsString = false;
        IsBool = true;
    }

    internal VanillaOptionData(Func<string> labelGetter, BoolOptionNames optionName)
    {
        BoolOptionName = optionName;
        LabelGetter = labelGetter;
        IsInt = IsFloat = IsString = false;
        IsBool = true;
    }

    internal VanillaOptionData(StringNames stringName, FloatOptionNames optionName, string valuePrefix = "",
        string valueSuffix = "")
    {
        StringName = stringName;
        FloatOptionName = optionName;
        ValuePrefix = valuePrefix;
        ValueSuffix = valueSuffix;
        IsBool = IsInt = IsString = false;
        IsFloat = true;
    }

    internal VanillaOptionData(StringNames stringName, Func<float> valueGetter, string valuePrefix = "",
        string valueSuffix = "")
    {
        StringName = stringName;
        ValueGetter = valueGetter != null ? () => valueGetter() : null;
        ValuePrefix = valuePrefix;
        ValueSuffix = valueSuffix;
        IsBool = IsInt = IsString = false;
        IsFloat = true;
    }

    internal VanillaOptionData(Func<string> labelGetter, Func<float> valueGetter, string valuePrefix = "",
        string valueSuffix = "")
    {
        LabelGetter = labelGetter;
        ValueGetter = valueGetter != null ? () => valueGetter() : null;
        ValuePrefix = valuePrefix;
        ValueSuffix = valueSuffix;
        IsBool = IsInt = IsString = false;
        IsFloat = true;
    }

    internal VanillaOptionData(Func<string> labelGetter, FloatOptionNames optionName, string valuePrefix = "",
        string valueSuffix = "")
    {
        FloatOptionName = optionName;
        LabelGetter = labelGetter;
        ValuePrefix = valuePrefix;
        ValueSuffix = valueSuffix;
        IsBool = IsInt = IsString = false;
        IsFloat = true;
    }

    internal VanillaOptionData(StringNames stringName, Int32OptionNames optionName, string valuePrefix = "",
        string valueSuffix = "")
    {
        StringName = stringName;
        IntOptionName = optionName;
        ValuePrefix = valuePrefix;
        ValueSuffix = valueSuffix;
        IsBool = IsFloat = IsString = false;
        IsInt = true;
    }

    internal VanillaOptionData(StringNames stringName, Func<int> valueGetter, string valuePrefix = "",
        string valueSuffix = "")
    {
        StringName = stringName;
        ValueGetter = valueGetter != null ? () => valueGetter() : null;
        ValuePrefix = valuePrefix;
        ValueSuffix = valueSuffix;
        IsBool = IsFloat = IsString = false;
        IsInt = true;
    }

    internal VanillaOptionData(Func<string> labelGetter, Func<int> valueGetter, string valuePrefix = "",
        string valueSuffix = "")
    {
        LabelGetter = labelGetter;
        ValueGetter = valueGetter != null ? () => valueGetter() : null;
        ValuePrefix = valuePrefix;
        ValueSuffix = valueSuffix;
        IsBool = IsFloat = IsString = false;
        IsInt = true;
    }

    internal VanillaOptionData(Func<string> labelGetter, Int32OptionNames optionName, string valuePrefix = "",
        string valueSuffix = "")
    {
        IntOptionName = optionName;
        LabelGetter = labelGetter;
        ValuePrefix = valuePrefix;
        ValueSuffix = valueSuffix;
        IsBool = IsFloat = IsString = false;
        IsInt = true;
    }

    internal bool IsBool { get; private set; }
    internal bool IsInt { get; private set; }
    internal bool IsFloat { get; private set; }
    internal bool IsString { get; private set; }

    internal string GetLabel()
    {
        return LabelGetter != null
            ? LabelGetter()
            : DestroyableSingleton<TranslationController>.Instance.GetString(StringName);
    }

    internal bool GetBool()
    {
        if (!UiVanillaOptionTab.IsOptionsReady) return false;
        if (!IsBool)
        {
            throw new Exception($"No boolean value in UiVanillaOption {StringName.ToString()}");
        }

        if (!BoolOptionName.HasValue) return (bool)ValueGetter();
        return GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionName.Value);
    }

    internal int GetInt()
    {
        if (!UiVanillaOptionTab.IsOptionsReady) return 0;
        if (!IsInt)
        {
            throw new Exception($"No int value in UiVanillaOption {StringName.ToString()}");
        }

        if (!IntOptionName.HasValue) return (int)ValueGetter();
        return GameOptionsManager.Instance.CurrentGameOptions.GetInt(IntOptionName.Value);
    }

    internal float GetFloat()
    {
        if (!UiVanillaOptionTab.IsOptionsReady) return 0f;
        if (!IsFloat)
        {
            throw new Exception($"No float value in UiVanillaOption {StringName.ToString()}");
        }

        if (!FloatOptionName.HasValue) return (float)ValueGetter();
        return GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionName.Value);
    }

    internal string GetString()
    {
        if (!UiVanillaOptionTab.IsOptionsReady) return string.Empty;
        if (IsString) return (string)ValueGetter();
        if (IsFloat) return $"{ValuePrefix}{GetFloat()}{ValueSuffix}";
        if (IsInt) return $"{ValuePrefix}{GetInt()}{ValueSuffix}";
        throw new Exception($"No stringify-able value in UiVanillaOption {StringName.ToString()}");
    }
}