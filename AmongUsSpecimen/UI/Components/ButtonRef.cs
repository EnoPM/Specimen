using System;
using UnityEngine;
using UnityEngine.UI;

namespace AmongUsSpecimen.UI.Components;

public class ButtonRef
{
    private readonly UniverseLib.UI.Models.ButtonRef _ref;

    public Action OnClick
    {
        get => _ref.OnClick;
        set => _ref.OnClick = value;
    }

    public Button Component => _ref.Component;

    public Text ButtonText => _ref.ButtonText;

    public GameObject GameObject => _ref.GameObject;

    public RectTransform Transform => _ref.Transform;

    public bool Enabled
    {
        get => _ref.Enabled;
        set => _ref.Enabled = value;
    }
    
    

    public ButtonRef(UniverseLib.UI.Models.ButtonRef buttonRef)
    {
        _ref = buttonRef;
    }
}