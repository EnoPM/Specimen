using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.UI.Components;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.Options;

internal class UiVanillaOptionTab : UiOptionTab
{
    private readonly List<VanillaOption> _options = new();
    private readonly GameObject _optionsContainer;
    
    internal UiVanillaOptionTab(GameObject parent, int width, int height) : base(parent, width, height)
    {
        var headerColor = UIPalette.Dark * 0.5f;
        headerColor.a = 1f;
        var titleContainer = UiFactory.CreateHorizontalGroup(_gameObject, "TitleContainer", false, false, true, true,
            bgColor: headerColor, childAlignment: TextAnchor.MiddleCenter);
        UiFactory.SetLayoutElement(titleContainer, Width, 50, 0, 0, 0, 0);
        var title = UiFactory.CreateLabel(titleContainer, "Title", DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameCustomSettings), TextAnchor.MiddleCenter, Color.black,
            true, 35);
        title.fontStyle = FontStyle.Bold;
        var titleObj = title.gameObject;
        var outline = titleObj.AddComponent<Outline>();
        outline.effectDistance = Vector2.one;
        outline.effectColor = Color.white;
        UiFactory.SetLayoutElement(titleObj, Width, 40, 0, 0, 0, 0);

        var layout = UiFactory.CreateScrollView(_gameObject, "Layout", out _optionsContainer, out _, minWidth: Width,
            minHeight: 300, spacing: 10, color: Palette.EnabledColor);
        layout.GetComponent<Image>().color = UIPalette.Dark;
        UiFactory.SetLayoutElement(layout, Width, Height - 50, 0, 0);
        
        Add(StringNames.GameMapName, RenderMapNameValue);
        Add(StringNames.GameNumImpostors, Int32OptionNames.NumImpostors);
        Add(StringNames.GameConfirmImpostor, BoolOptionNames.ConfirmImpostor);
        Add(StringNames.GameNumMeetings, Int32OptionNames.NumEmergencyMeetings);
        Add(StringNames.GameEmergencyCooldown, Int32OptionNames.EmergencyCooldown, suffix: "s");
        Add(StringNames.GameDiscussTime, Int32OptionNames.DiscussionTime, suffix: "s");
        Add(StringNames.GameVotingTime, Int32OptionNames.VotingTime, suffix: "s");
        Add(StringNames.GameAnonymousVotes, BoolOptionNames.AnonymousVotes);
        Add(StringNames.GamePlayerSpeed, FloatOptionNames.PlayerSpeedMod, suffix: "x");
        Add(StringNames.GameCrewLight, FloatOptionNames.CrewLightMod, suffix: "x");
        Add(StringNames.GameImpostorLight, FloatOptionNames.ImpostorLightMod, suffix: "x");
        Add(StringNames.GameKillCooldown, FloatOptionNames.KillCooldown, suffix: "s");
        Add(StringNames.GameKillDistance, RenderKillDistanceValue);
        Add(StringNames.GameTaskBarMode, RenderTaskBarModeValue);
        Add(StringNames.GameVisualTasks, BoolOptionNames.VisualTasks);
        Add(StringNames.GameCommonTasks, Int32OptionNames.NumCommonTasks);
        Add(StringNames.GameLongTasks, Int32OptionNames.NumLongTasks);
        Add(StringNames.GameShortTasks, Int32OptionNames.NumShortTasks);
    }

    private static string RenderMapNameValue()
    {
        if (!IsOptionsReady) return string.Empty;
        var mapId = GameOptionsManager.Instance.CurrentGameOptions.MapId != 0 || !Constants.ShouldFlipSkeld()
            ? GameOptionsManager.Instance.CurrentGameOptions.MapId
            : 3;
        return Constants.MapNames[mapId];
    }

    private static string RenderKillDistanceValue()
    {
        if (!IsOptionsReady) return string.Empty;
        var killDistance = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.KillDistance);
        var stringName = (StringNames)(204 + killDistance);
        return DestroyableSingleton<TranslationController>.Instance.GetString(stringName);
    }
    
    private static string RenderTaskBarModeValue()
    {
        if (!IsOptionsReady) return string.Empty;
        var taskBarMode = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.TaskBarMode);
        var stringName = (StringNames)(277 + taskBarMode);
        return DestroyableSingleton<TranslationController>.Instance.GetString(stringName);
    }
    
    public override void UiUpdate()
    {
        base.UiUpdate();
        foreach (var option in _options)
        {
            option.UiUpdate();
        }
    }
    
    internal static bool IsOptionsReady => GameOptionsManager.Instance != null && GameOptionsManager.Instance.HasOptions && GameData.Instance;
    
    private void Add(StringNames stringName, Func<string> valueGetter)
    {
        AddOption(new VanillaOptionData(stringName, valueGetter));
    }

    private void Add(StringNames stringName, BoolOptionNames optionName)
    {
        AddOption(new VanillaOptionData(stringName, optionName));
    }
    
    private void Add(StringNames stringName, Func<bool> valueGetter)
    {
        AddOption(new VanillaOptionData(stringName, valueGetter));
    }
    
    private void Add(StringNames stringName, FloatOptionNames optionName, string prefix = "", string suffix = "")
    {
        AddOption(new VanillaOptionData(stringName, optionName, prefix, suffix));
    }
    
    private void Add(StringNames stringName, Func<float> valueGetter, string prefix = "", string suffix = "")
    {
        AddOption(new VanillaOptionData(stringName, valueGetter, prefix, suffix));
    }
    
    private void Add(StringNames stringName, Int32OptionNames optionName, string prefix = "", string suffix = "")
    {
        AddOption(new VanillaOptionData(stringName, optionName, prefix, suffix));
    }
    
    private void Add(StringNames stringName, Func<int> valueGetter, string prefix = "", string suffix = "")
    {
        AddOption(new VanillaOptionData(stringName, valueGetter, prefix, suffix));
    }

    private void AddOption(VanillaOptionData data)
    {
        if (_options.Any(x => x.Data.StringName == data.StringName))
        {
            Specimen.Instance.Log.LogWarning($"UiVanillaOption Cannot add option: Option {data.StringName.ToString()} already added.");
            return;
        }
        _options.Add(new VanillaOption(_optionsContainer, Width - 20, data));
    }
    
    internal class VanillaOption
    {
        private readonly Text Label;
        private readonly Text ValueText;
        private readonly GameObject ValueObject;
        private readonly Image BoolImage;
        public readonly VanillaOptionData Data;

        internal VanillaOption(GameObject parent, int width, VanillaOptionData data)
        {
            Specimen.Instance.Log.LogMessage($"Create VanillaOption for option {data.StringName.ToString()}");
            Data = data;

            var container = UiFactory.CreateHorizontalGroup(parent, "VanillaOption", false, false, true, true, 5,
                Vector4.zero, UIPalette.LightDanger, TextAnchor.MiddleLeft);
            container.GetComponent<Image>().enabled = false;
            UiFactory.SetLayoutElement(container, width, 30, 0, 0, 0, 0);

            Label = UiFactory.CreateLabel(container, "Label", Data.StringName.ToString(), TextAnchor.MiddleLeft, Color.white, true, 25);
            UiFactory.SetLayoutElement(Label.gameObject, width - 185, 30, 0, 0, 0, 0);

            if (Data.IsBool)
            {
                var boolContainer = UiFactory.CreateHorizontalGroup(container, "BoolContainer", false, false, true, true);
                boolContainer.GetComponent<Image>().enabled = false;
                UiFactory.SetLayoutElement(boolContainer, 180, 30, 0, 0, 0, 0);
                ValueObject = UIFactory.CreateHorizontalGroup(boolContainer, "ImageContainer", false, false, true, true,
                    bgColor: Palette.EnabledColor, childAlignment: TextAnchor.MiddleLeft);
                UiFactory.SetLayoutElement(ValueObject, 25, 25, 0, 0, 0, 0);
                BoolImage = ValueObject.GetComponent<Image>();
                BoolImage.sprite = AmongUsCheckbox.UncheckedSprite;
            }
            else
            {
                ValueText = UiFactory.CreateLabel(container, "ValueText", string.Empty, TextAnchor.MiddleLeft,
                    Color.white, true, 25);
                ValueObject = ValueText.gameObject;
                UiFactory.SetLayoutElement(ValueObject, 180, 30, 0, 0, 0, 0);
            }
        }

        internal void UiUpdate()
        {
            Label.text = Data.GetLabel();
            if (Data.IsBool)
            {
                BoolImage.sprite = Data.GetBool() ? AmongUsCheckbox.CheckedSprite : AmongUsCheckbox.UncheckedSprite;
            }
            else
            {
                ValueText.text = Data.GetString();
            }
        }
    }   
    
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
        
        internal VanillaOptionData(StringNames stringName, FloatOptionNames optionName, string valuePrefix = "", string valueSuffix = "")
        {
            StringName = stringName;
            FloatOptionName = optionName;
            ValuePrefix = valuePrefix;
            ValueSuffix = valueSuffix;
            IsBool = IsInt = IsString = false;
            IsFloat = true;
        }
        
        internal VanillaOptionData(StringNames stringName, Func<float> valueGetter, string valuePrefix = "", string valueSuffix = "")
        {
            StringName = stringName;
            ValueGetter = valueGetter != null ? () => valueGetter() : null;
            ValuePrefix = valuePrefix;
            ValueSuffix = valueSuffix;
            IsBool = IsInt = IsString = false;
            IsFloat = true;
        }
        
        internal VanillaOptionData(Func<string> labelGetter, Func<float> valueGetter, string valuePrefix = "", string valueSuffix = "")
        {
            LabelGetter = labelGetter;
            ValueGetter = valueGetter != null ? () => valueGetter() : null;
            ValuePrefix = valuePrefix;
            ValueSuffix = valueSuffix;
            IsBool = IsInt = IsString = false;
            IsFloat = true;
        }
        
        internal VanillaOptionData(Func<string> labelGetter, FloatOptionNames optionName, string valuePrefix = "", string valueSuffix = "")
        {
            FloatOptionName = optionName;
            LabelGetter = labelGetter;
            ValuePrefix = valuePrefix;
            ValueSuffix = valueSuffix;
            IsBool = IsInt = IsString = false;
            IsFloat = true;
        }
        
        internal VanillaOptionData(StringNames stringName, Int32OptionNames optionName, string valuePrefix = "", string valueSuffix = "")
        {
            StringName = stringName;
            IntOptionName = optionName;
            ValuePrefix = valuePrefix;
            ValueSuffix = valueSuffix;
            IsBool = IsFloat = IsString = false;
            IsInt = true;
        }
        
        internal VanillaOptionData(StringNames stringName, Func<int> valueGetter, string valuePrefix = "", string valueSuffix = "")
        {
            StringName = stringName;
            ValueGetter = valueGetter != null ? () => valueGetter() : null;
            ValuePrefix = valuePrefix;
            ValueSuffix = valueSuffix;
            IsBool = IsFloat = IsString = false;
            IsInt = true;
        }
        
        internal VanillaOptionData(Func<string> labelGetter, Func<int> valueGetter, string valuePrefix = "", string valueSuffix = "")
        {
            LabelGetter = labelGetter;
            ValueGetter = valueGetter != null ? () => valueGetter() : null;
            ValuePrefix = valuePrefix;
            ValueSuffix = valueSuffix;
            IsBool = IsFloat = IsString = false;
            IsInt = true;
        }
        
        internal VanillaOptionData(Func<string> labelGetter, Int32OptionNames optionName, string valuePrefix = "", string valueSuffix = "")
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
            return LabelGetter != null ? LabelGetter() : DestroyableSingleton<TranslationController>.Instance.GetString(StringName);
        }

        internal bool GetBool()
        {
            if (!IsOptionsReady) return false;
            if (!IsBool)
            {
                throw new Exception($"No boolean value in UiVanillaOption {StringName.ToString()}");
            }
            if (!BoolOptionName.HasValue) return (bool)ValueGetter();
            return GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionName.Value);
        }

        internal int GetInt()
        {
            if (!IsOptionsReady) return 0;
            if (!IsInt)
            {
                throw new Exception($"No int value in UiVanillaOption {StringName.ToString()}");
            }
            if (!IntOptionName.HasValue) return (int)ValueGetter();
            return GameOptionsManager.Instance.CurrentGameOptions.GetInt(IntOptionName.Value);
        }
        
        internal float GetFloat()
        {
            if (!IsOptionsReady) return 0f;
            if (!IsFloat)
            {
                throw new Exception($"No float value in UiVanillaOption {StringName.ToString()}");
            }
            if (!FloatOptionName.HasValue) return (float)ValueGetter();
            return GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionName.Value);
        }

        internal string GetString()
        {
            if (!IsOptionsReady) return string.Empty;
            if (IsString) return (string)ValueGetter();
            if (IsFloat) return $"{ValuePrefix}{GetFloat()}{ValueSuffix}";
            if (IsInt) return $"{ValuePrefix}{GetInt()}{ValueSuffix}";
            throw new Exception($"No stringify-able value in UiVanillaOption {StringName.ToString()}");
        }
    }
}