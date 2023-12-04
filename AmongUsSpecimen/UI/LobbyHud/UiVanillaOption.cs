using AmongUsSpecimen.UI.Components;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.LobbyHud;

internal class UiVanillaOption
{
    private readonly Text Label;
    private readonly Text ValueText;
    private readonly GameObject ValueObject;
    private readonly Image BoolImage;
    public readonly VanillaOptionData Data;

    internal UiVanillaOption(GameObject parent, int width, VanillaOptionData data)
    {
        Data = data;

        var container = UiFactory.CreateHorizontalGroup(parent, "VanillaOption", false, false, true, true, 5,
            Vector4.zero, UIPalette.LightDanger, TextAnchor.MiddleLeft);
        container.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(container, width, 30, 0, 0, 0, 0);

        Label = UiFactory.CreateLabel(container, "Label", Data.StringName.ToString(), TextAnchor.MiddleLeft,
            Color.white, true, 25);
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