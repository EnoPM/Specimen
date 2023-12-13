using AmongUsSpecimen.ModOptions;
using AmongUsSpecimen.UI.Components;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.LobbyHud;

internal class UiCustomOption
{
    private const int HeaderPaddingTop = 10;
    internal readonly BaseModOption Option;
    private readonly GameObject _container;
    private readonly Text Label;
    private readonly Text ValueText;
    private readonly GameObject ValueObject;
    private readonly Image BoolImage;
    
    internal UiCustomOption(GameObject parent, int width, BaseModOption option)
    {
        Option = option;
        var height = 30;
        var paddingTop = 0f;
        if (Option.IsHeader)
        {
            height += HeaderPaddingTop;
            paddingTop += HeaderPaddingTop;
        }
        _container = UiFactory.CreateHorizontalGroup(parent, "VanillaOption", false, false, true, true, 5,
            new Vector4(paddingTop, 0f, 0f, 0f), UIPalette.LightDanger, TextAnchor.MiddleLeft);
        _container.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(_container, width, height, 0, 0, 0, 0);

        var btn = _container.AddComponent<Button>();
        btn.onClick.AddListener(OnOptionClick);
        
        Label = UiFactory.CreateLabel(_container, "Label", Option.GetDisplayName(5), TextAnchor.MiddleLeft,
            LabelColor, true, 25);
        Label.resizeTextForBestFit = true;
        Label.fontStyle = Option.IsHeader ? FontStyle.Bold : FontStyle.Normal;
        UiFactory.SetLayoutElement(Label.gameObject, width - 185, 30, 0, 0, 0, 0);

        if (Option.Type == OptionType.Boolean)
        {
            var boolContainer = UiFactory.CreateHorizontalGroup(_container, "BoolContainer", false, false, true, true);
            boolContainer.GetComponent<Image>().enabled = false;
            UiFactory.SetLayoutElement(boolContainer, 145, 30, 0, 0, 0, 0);
            ValueObject = UIFactory.CreateHorizontalGroup(boolContainer, "ImageContainer", false, false, true, true,
                bgColor: Palette.EnabledColor, childAlignment: TextAnchor.MiddleLeft);
            UiFactory.SetLayoutElement(ValueObject, 25, 25, 0, 0, 0, 0);
            BoolImage = ValueObject.GetComponent<Image>();
            BoolImage.sprite = Option.CurrentSelection > 0 ? AmongUsCheckbox.CheckedSprite : AmongUsCheckbox.UncheckedSprite;
        }
        else
        {
            ValueText = UiFactory.CreateLabel(_container, "ValueText", Option.DisplayUiValue, TextAnchor.MiddleLeft,
                Color.white, true, 25);
            ValueObject = ValueText.gameObject;
            ValueText.resizeTextForBestFit = true;
            ValueText.horizontalOverflow = HorizontalWrapMode.Wrap;
            UiFactory.SetLayoutElement(ValueObject, 145, 30, 0, 0, 0, 0);
        }

        Option.UiOption = this;
        Option.ValueChanged += UiUpdate;
    }

    private void OnOptionClick()
    {
        Option.OnUiLabelClick?.Invoke();
    }

    private Color LabelColor => Option.Restriction switch
    {
        OptionRestriction.Local => Color.cyan,
        OptionRestriction.Private => Color.red,
        OptionRestriction.Public => Color.white,
        _ => Color.gray
    };

    internal void UiUpdate()
    {
        Label.text = Option.GetDisplayName(5);
        Label.color = LabelColor;
        Label.fontStyle = Option.IsHeader ? FontStyle.Bold : FontStyle.Normal;
        if (Option.Type == OptionType.Boolean)
        {
            BoolImage.sprite = Option.CurrentSelection > 0 ? AmongUsCheckbox.CheckedSprite : AmongUsCheckbox.UncheckedSprite;
        }
        else
        {
            ValueText.text = Option.DisplayUiValue;
        }

        foreach (var child in Option.Children)
        {
            child.UiOption?.UiUpdate();
        }
        
        SetActive(Option.IsParentEnabled);
    }

    internal void SetActive(bool value)
    {
        _container.SetActive(value);
    }
}