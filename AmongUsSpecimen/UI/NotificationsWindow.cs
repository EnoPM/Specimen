using AmongUsSpecimen.UI.Components;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI;

internal class NotificationsWindow : UiWindow
{
    public override string Name => nameof(NotificationsWindow);
    public override int MinWidth => 250;
    public override int MinHeight => Screen.height;
    public override bool DisableGameClickWhenOpened => false;
    protected override bool DisplayTitleBar => false;
    protected override Color BackgroundColor => UIPalette.Transparent;
    protected override bool DisplayByDefault => true;
    protected override Positions Position => Positions.MiddleRight;

    private GameObject _container;

    protected override void ConstructWindowContent()
    {
        _container = UiFactory.CreateVerticalGroup(ContentRoot, "Container",
            false, false,
            true, true,
            bgColor: UIPalette.Transparent,
            childAlignment: TextAnchor.MiddleLeft,
            spacing: 5,
            padding: Vector4.zero
        );
        UiFactory.SetLayoutElement(_container, MinWidth, MinHeight, 0, 0);
        _container.GetComponent<Image>().enabled = false;
    }

    internal GameObject CreateNotificationObject(string name)
    {
        var obj = UiFactory.CreateVerticalGroup(
            _container,
            name,
            false,
            false,
            true,
            true,
            0,
            Vector4.zero,
            UIPalette.Dark,
            TextAnchor.UpperLeft
        );
        UiFactory.SetLayoutElement(obj, Notification.Width, Notification.Height, 0, 0);
        return obj;
    }
}