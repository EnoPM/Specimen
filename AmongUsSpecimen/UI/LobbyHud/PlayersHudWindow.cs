using System.Collections.Generic;
using AmongUsSpecimen.VersionCheck;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.LobbyHud;

public class PlayersHudWindow : UiWindow
{
    internal const int MaxPlayerCount = 15;
    
    public override string Name => "Players HUD";
    protected override string Title => $"<b><color=#5925b3>Specimen</color> {Name}</b>";
    public override int MinWidth => 500;
    public override int MinHeight => Screen.height - LobbyHudWindow.Height;
    public override bool HasOverlay => false;
    protected override bool DisplayByDefault => false;
    public override bool DisableGameClickWhenOpened => false;
    protected override bool DisplayTitleBar => false;
    public override bool AlwaysOnTop => true;
    protected override Color BackgroundColor => UIPalette.Transparent;
    protected override Positions Position => Positions.TopLeft;

    internal readonly List<PlayerVersionCheck> VersionChecks = [];
    
    protected override void ConstructWindowContent()
    {
        var scrollbar = UiFactory.CreateScrollView(ContentRoot, "ScrollBar", out var container, out _, minHeight: MinHeight);
        UiFactory.SetLayoutElement(container, MinWidth, MinHeight, 0, 0);

        for (var i = 0; i < MaxPlayerCount; i++)
        {
            var playerContainer = UiFactory.CreateHorizontalGroup(
                container,
                "PlayerContainer",
                false,
                false,
                true,
                true,
                0,
                Vector4.zero,
                UIPalette.Transparent,
                TextAnchor.UpperLeft);
            playerContainer.GetComponent<Image>().enabled = false;
            UiFactory.SetLayoutElement(playerContainer, PlayerVersionCheck.Width, PlayerVersionCheck.Height, 0, 0);
            VersionChecks.Add(new PlayerVersionCheck(playerContainer));
        }
    }
}