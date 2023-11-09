using AmongUsSpecimen.UI.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.Components;

public class WindowHeader
{
    private readonly GameObject _bar;
    private readonly Text _title;
    private readonly GameObject _closeButtonContainer;
    private readonly ButtonRef _closeButton;
    
    public WindowHeader(UiWindow window, string title)
    {
        _bar = UiFactory.CreateHorizontalGroup(window.ContentRoot, "TitleBar", 
            false, true, true, true, 2,
            new Vector4(5, 5, 10, 2), UIPalette.Black);
        UiFactory.SetLayoutElement(_bar, minHeight: 25, flexibleHeight: 0);
        
        _title =  UiFactory.CreateLabel(_bar, "TitleBar", title, TextAnchor.MiddleLeft, UIPalette.Secondary,
            true, 20);
        UiFactory.SetLayoutElement(_title.gameObject, 50, 25, 9999, 0);

        _closeButtonContainer = UiFactory.CreateUIObject("CloseHolder", _bar);
        UiFactory.SetLayoutGroup<HorizontalLayoutGroup>(_closeButtonContainer, false, false, true, true, 3,
            childAlignment: TextAnchor.MiddleRight);
        
        _closeButton = UiFactory.CreateButton(_closeButtonContainer, "CloseButton", "╳");
        _closeButton.ButtonText.fontSize = 25;
        
        UiFactory.SetLayoutElement(_closeButton.Component.gameObject, minHeight: 30, minWidth: 30, flexibleWidth: 0);
        _closeButton.Component.SetColors(normal: UIPalette.Danger, hover: UIPalette.LightDanger, pressed: UIPalette.LightDanger, focused: UIPalette.Danger);
        
        _closeButton.OnClick += window.TriggerClose;
    }

    public void SetActive(bool value)
    {
        _bar.SetActive(value);
    }

    public void SetText(string text)
    {
        _title.text = text;
    }

    public void SetCloseButtonActive(bool value)
    {
        _closeButtonContainer.SetActive(false);
    }

    public void SetCloseButtonEnabled(bool enabled)
    {
        _closeButton.Enabled = enabled;
    }
}