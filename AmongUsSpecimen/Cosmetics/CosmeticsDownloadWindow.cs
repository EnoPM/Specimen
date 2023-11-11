using AmongUsSpecimen.UI;
using UnityEngine;
using UniverseLib.UI;

namespace AmongUsSpecimen.Cosmetics;

public class CosmeticsDownloadWindow : UiWindow
{
    public override string Name => nameof(CosmeticsDownloadWindow);
    public override int MinWidth => 400;
    public override int MinHeight => 200;
    public override bool DisableGameClickWhenOpened => false;
    protected override bool DisplayTitleBar => false;
    protected override Color BackgroundColor => UIPalette.Transparent;
    //protected override Color BackgroundColor => Color.red;
    public override bool AlwaysOnTop => false;
    protected override bool DisplayByDefault => false;
    protected override Positions Position => Positions.MiddleRight;


    protected override void ConstructWindowContent()
    {
        var container = UiFactory.CreateVerticalGroup(UIRoot, "Container", false, false, false, false, bgColor: UIPalette.Transparent);
        UiFactory.SetLayoutElement(container, MinWidth, MinHeight, 0, 0);

        var title = UiFactory.CreateLabel(container, "Title", "Cosmetics assets download in progress",
            TextAnchor.MiddleLeft, Color.white);
        title.fontSize = 20;
        UiFactory.SetLayoutElement(title.gameObject, MinWidth, 25, 0, 0);
    }
}