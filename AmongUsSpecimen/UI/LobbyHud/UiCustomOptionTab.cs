using System.Collections.Generic;
using System.Linq;
using AmongUsSpecimen.ModOptions;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.LobbyHud;

internal class UiCustomOptionTab : UiOptionTab
{
    internal readonly ModOptionTab Tab;
    private readonly GameObject _optionsContainer;
    private readonly List<UiCustomOption> _options = new();

    internal UiCustomOptionTab(GameObject parent, int width, int height, ModOptionTab tab) : base(parent, width,
        height)
    {
        Tab = tab;
        var headerColor = UIPalette.Dark * 0.5f;
        headerColor.a = 1f;

        var titleContainer = UiFactory.CreateHorizontalGroup(_gameObject, "TitleContainer", false, false, true, true,
            bgColor: headerColor, childAlignment: TextAnchor.MiddleCenter);
        UiFactory.SetLayoutElement(titleContainer, Width, 50, 0, 0, 0, 0);

        var title = UiFactory.CreateLabel(titleContainer, "Title", Tab.Title, TextAnchor.MiddleCenter, Color.black,
            true, 35);
        title.fontStyle = FontStyle.Bold;
        var titleObj = title.gameObject;
        var outline = titleObj.AddComponent<Outline>();
        outline.effectDistance = Vector2.one;
        outline.effectColor = Color.white;
        UiFactory.SetLayoutElement(titleObj, Width, 40, 0, 0, 0, 0);

        var layout = UiFactory.CreateScrollView(_gameObject, "Layout", out _optionsContainer, out _, minWidth: Width,
            minHeight: Height - 50, spacing: 10, color: Palette.EnabledColor, contentAlignment: TextAnchor.UpperLeft);
        layout.GetComponent<Image>().color = UIPalette.Dark;
        UiFactory.SetLayoutElement(layout, Width, Height - 50, 0, 0);

        foreach (var option in Tab.Options.Where(x => x.Parent == null || x.Parent.Tab != Tab))
        {
            CreateUiOption(option);
        }
    }

    private void CreateUiOption(BaseModOption option)
    {
        var result = new UiCustomOption(_optionsContainer, Width, option);
        result.SetActive(option.IsParentEnabled);
        _options.Add(result);
        foreach (var subOption in option.Children.Where(x => x.Tab == option.Tab))
        {
            CreateUiOption(subOption);
        }
    }
}