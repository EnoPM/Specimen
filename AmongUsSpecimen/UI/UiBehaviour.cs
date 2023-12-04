using AmongUsSpecimen.VersionCheck;
using InnerNet;
using UnityEngine;

namespace AmongUsSpecimen.UI;

internal class UiBehaviour : MonoBehaviour
{
    private int _cachedHeight;
    private int _cachedWidth;
    private InnerNetClient.GameStates _cachedGameState = InnerNetClient.GameStates.NotJoined;
    private int _cachedLocalClientId = -1;
    private int _cachedLocalOwnerId = -1;

    internal InnerNetClient.GameStates GetCurrentGameState() => _cachedGameState;

    private void Start()
    {
        _cachedHeight = Screen.height;
        _cachedWidth = Screen.width;
    }

    private void Update()
    {
        CheckAndUpdateScreenSize();
        CheckUpdatedGameState();
        CheckUpdatedLocalClientId();
        CheckUpdatedLocalOwnerId();
        NotificationManager.UiUpdate();
    }

    private void CheckAndUpdateScreenSize()
    {
        var height = Screen.height;
        var width = Screen.width;
        if (height == _cachedHeight && width == _cachedWidth) return;
        Specimen.Instance.Log.LogMessage($"Screen size updated!");
        _cachedHeight = height;
        _cachedWidth = width;
        UiManager.EnsureWindowValidPositions();
    }

    private void CheckUpdatedGameState()
    {
        if (!AmongUsClient.Instance) return;
        var gameState = AmongUsClient.Instance.GameState;
        if (gameState != _cachedGameState)
        {
            _cachedGameState = gameState;
            Specimen.Instance.Log.LogMessage($"GameState updated: {_cachedGameState.ToString()}");
        }
    }

    private void CheckUpdatedLocalClientId()
    {
        if (!AmongUsClient.Instance) return;
        var clientId = AmongUsClient.Instance.ClientId;
        if (clientId != _cachedLocalClientId)
        {
            _cachedLocalClientId = clientId;
            Specimen.Instance.Log.LogMessage($"ClientId updated {_cachedLocalClientId}");
        }
    }
    
    private void CheckUpdatedLocalOwnerId()
    {
        if (!PlayerControl.LocalPlayer) return;
        var ownerId = PlayerControl.LocalPlayer.OwnerId;
        if (ownerId != _cachedLocalOwnerId)
        {
            VersionHandshakeManager.LocalHandshake.ClientId = _cachedLocalOwnerId = ownerId;
        }
    }
}