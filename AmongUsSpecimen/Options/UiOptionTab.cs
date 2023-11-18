using AmongUsSpecimen.UI;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.Options;

public class UiOptionTab
{
    protected readonly GameObject _gameObject;
    protected readonly int Width;
    protected readonly int Height;
    public UiOptionTab(GameObject parent, int width, int height)
    {
        Width = width;
        Height = height;
        _gameObject = UiFactory.CreateVerticalGroup(
            parent,
            "TabContainer",
            false,
            false,
            true,
            true,
            0,
            Vector4.zero,
            UIPalette.Transparent,
            TextAnchor.UpperLeft
        );
        _gameObject.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(_gameObject, Width, Height, 0, 9999);
        // obj.SetActive(false);
    }

    public void SetActive(bool value) => _gameObject.SetActive(value);

    public virtual void UiUpdate()
    {
        
    }
}