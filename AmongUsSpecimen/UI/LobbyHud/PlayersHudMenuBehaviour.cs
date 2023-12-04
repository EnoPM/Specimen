using System;
using System.Linq;
using AmongUsSpecimen.Utils;
using UnityEngine;

namespace AmongUsSpecimen.UI.LobbyHud;

[RegisterMonoBehaviour]
internal class PlayersHudMenuBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
    internal PlayersHudWindow Window { get; set; }

    internal void Start()
    {
        Window = UiManager.RegisterWindow<PlayersHudWindow>();
    }

    internal void Update()
    {
        if (!PlayerControl.LocalPlayer || PlayerControl.AllPlayerControls == null) return;
        var sortedPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Data != null)
            .OrderBy(x => x.Data.PlayerName).ToArray();

        for (var i = 0; i < Window.VersionChecks.Count; i++)
        {
            Window.VersionChecks[i].Refresh(sortedPlayers.Length > i ? sortedPlayers[i] : null);
            // Window.VersionChecks[i].Refresh(sortedPlayers.Length > 0 && i < 10 ? sortedPlayers[0] : null);
        }
    }
    
    internal bool ShouldBlockGameStart()
    {
        return Window.VersionChecks.Any(x => x.ShouldBlockGameStart);
    }

    internal void SetActive(bool value) => Window?.SetActive(value);
}