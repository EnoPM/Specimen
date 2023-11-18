using System;
using System.Reflection;
using AmongUsSpecimen.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.Components;

public sealed class AmongUsCheckbox
{
    internal static Sprite CheckedSprite => SpecimenSprites.SuccessIcon;
    internal static Sprite UncheckedSprite => SpecimenSprites.ErrorIcon;
    
    public readonly GameObject GameObject;
    public readonly Text Label;
    public event Action<bool> ValueChanged; 
    public bool Value { get; private set; }
    
    private readonly GameObject _checkbox;
    private readonly Image _checkboxImage;
    
    public AmongUsCheckbox(GameObject parent, int checkboxSize = 20, int maxWidth = 100, int spacing = 5, Vector4? padding = null, bool defaultValue = false)
    {
        GameObject = UiFactory.CreateHorizontalGroup(parent, nameof(AmongUsCheckbox),
            false, false, true, true, spacing, padding ?? Vector4.zero, UIPalette.LightDanger, TextAnchor.MiddleLeft);
        GameObject.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(GameObject, maxWidth, checkboxSize, 0, 0);
        _checkbox = UiFactory.CreateVerticalGroup(GameObject, "Checkbox", false, false, true, true, 0, Vector4.zero, Palette.EnabledColor);
        UiFactory.SetLayoutElement(_checkbox, checkboxSize, checkboxSize, 0, 0, 0, 0);
        _checkboxImage = _checkbox.GetComponent<Image>();
        _checkboxImage.sprite = defaultValue ? CheckedSprite : UncheckedSprite;
        var button = GameObject.AddComponent<Button>();
        button.onClick.AddListener((UnityAction)OnButtonClick);
        Value = defaultValue;
        Label = UiFactory.CreateLabel(GameObject, "Label", string.Empty, TextAnchor.MiddleLeft, Color.white, true, checkboxSize);
    }

    private void OnButtonClick()
    {
        SetChecked(!Value);
    }

    public void SetChecked(bool value)
    {
        if (value == Value) return;
        Value = value;
        _checkboxImage.sprite = Value ? CheckedSprite : UncheckedSprite;
        ValueChanged?.Invoke(Value);
    }
}