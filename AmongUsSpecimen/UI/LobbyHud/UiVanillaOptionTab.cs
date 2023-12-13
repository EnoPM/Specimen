using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using AmongUsSpecimen.Utils;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.LobbyHud;

internal class UiVanillaOptionTab : UiOptionTab
{
    private readonly List<UiVanillaOption> _options = new();
    private readonly GameObject _optionsContainer;
    
    internal UiVanillaOptionTab(GameObject parent, int width, int height) : base(parent, width, height)
    {
        var headerColor = UIPalette.Dark * 0.5f;
        headerColor.a = 1f;
        var titleContainer = UiFactory.CreateHorizontalGroup(_gameObject, "TitleContainer", false, false, true, true,
            bgColor: headerColor, childAlignment: TextAnchor.MiddleCenter);
        UiFactory.SetLayoutElement(titleContainer, Width, 50, 0, 0, 0, 0);
        var title = UiFactory.CreateLabel(titleContainer, "Title", "Vanilla Settings", TextAnchor.MiddleCenter, Color.black,
            true, 35);
        title.fontStyle = FontStyle.Bold;
        var titleObj = title.gameObject;
        var outline = titleObj.AddComponent<Outline>();
        outline.effectDistance = Vector2.one;
        outline.effectColor = Color.white;
        UiFactory.SetLayoutElement(titleObj, Width, 40, 0, 0, 0, 0);

        var layout = UiFactory.CreateScrollView(_gameObject, "Layout", out _optionsContainer, out _, minWidth: Width,
            minHeight: 300, spacing: 10, color: Palette.EnabledColor, contentAlignment: TextAnchor.UpperLeft);
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
        if (!ResourceHelpers.TranslationsInitialized)
        {
            return stringName.ToString();
        }
        return DestroyableSingleton<TranslationController>.Instance.GetString(stringName);
    }
    
    private static string RenderTaskBarModeValue()
    {
        if (!IsOptionsReady) return string.Empty;
        var taskBarMode = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.TaskBarMode);
        var stringName = (StringNames)(277 + taskBarMode);
        if (!ResourceHelpers.TranslationsInitialized)
        {
            return stringName.ToString();
        }
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
        _options.Add(new UiVanillaOption(_optionsContainer, Width - 20, data));
    }
}