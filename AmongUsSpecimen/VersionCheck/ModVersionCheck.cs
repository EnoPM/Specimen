using AmongUsSpecimen.UI;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.VersionCheck;

internal class ModVersionCheck
{
    private readonly GameObject _container;
    private readonly Text _label;
    private readonly Image _icon;

    public ModVersionCheck(GameObject modsContainer, Color color, string text = "")
    {
        _container = UiFactory.CreateHorizontalGroup(modsContainer, "ModVersionCheckContainer", false, false, true, true,
            spacing: 5, padding: new Vector4(5f, 5f, 5f, 5f), UIPalette.Transparent, TextAnchor.MiddleLeft);
        _container.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(_container, PlayerVersionCheck.Width, 40);

        var iconContainer =
            UiFactory.CreateVerticalGroup(_container, "ModVersionCheckIcon", false, false, false, false, 0, Vector4.zero, Palette.EnabledColor, TextAnchor.MiddleCenter);
        UiFactory.SetLayoutElement(iconContainer, 30, 30, 0, 0, 0, 0);
        _icon = iconContainer.GetComponent<Image>();
        
        _label = UiFactory.CreateLabel(_container, "ModVersionCheckLabel", text, color: color, fontSize: 15, alignment: TextAnchor.MiddleLeft);
        var gameObject = _label.gameObject;
        UIFactory.SetLayoutElement(gameObject, PlayerVersionCheck.Width - 45, 40, 0, 0);
    }

    internal void Destroy()
    {
        Object.Destroy(_label.gameObject);
    }

    internal void SetTextAndIcon(string text, Color? color = null, Sprite icon = null)
    {
        _label.text = text;
        if (color.HasValue)
        {
            _label.color = color.Value;
        }
        _icon.sprite = icon;
    }

    internal void SetActive(bool value)
    {
        _container.SetActive(value);
    }
}