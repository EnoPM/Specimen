using UnityEngine;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.Components;

public class OverlayWindow : UiWindow
{
    public override string Name => "UiOverlay";
    public override int MinWidth => Screen.width;
    public override int MinHeight => Screen.height;
    public override Vector2 DefaultAnchorMin => new(0.125f, 0.175f);
    public override Vector2 DefaultAnchorMax => new(0.325f, 0.925f);
    public override Vector2 DefaultPosition => new(0f, 0f);
    public override bool CanDragAndResize => false;

    protected override void ConstructWindowContent()
    {
        UiFactory.CreateLabel(ContentRoot, "SpecimenCredits", "Powered by AmongUsSpecimen");
    }

    protected override bool DisplayTitleBar => false;
    protected override Color BackgroundColor => new(0f, 0f, 0f, 0.6f);

    protected override Positions Position => Positions.TopLeft;
}