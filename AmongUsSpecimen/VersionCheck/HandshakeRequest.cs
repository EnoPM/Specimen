using System;
using System.Collections;
using AmongUsSpecimen.UI;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;

namespace AmongUsSpecimen.VersionCheck;

internal class HandshakeRequest
{
    private bool IsBusy;
    private PlayerControl Player;
    private DateTime ExpiresAt;
    private bool HasResponse;

    internal void Run(PlayerControl player)
    {
        if (IsBusy) return;
        Player = player;
        ExpiresAt = DateTime.UtcNow.Add(TimeSpan.FromSeconds(10));
        HasResponse = false;
        Specimen.Instance.Log.LogMessage($"Run HandshakeRequest from {PlayerControl.LocalPlayer.Data?.PlayerName} {PlayerControl.LocalPlayer.OwnerId} to {Player.Data?.PlayerName} ({Player.OwnerId})");
        UiManager.Behaviour.StartCoroutine(CoSendRequestAndWaitForResponseUnderTimeout());
    }

    internal void TriggerResponse()
    {
        HasResponse = true;
        Specimen.Instance.Log.LogMessage($"Reply to HandshakeRequest from {PlayerControl.LocalPlayer.Data?.PlayerName} to {Player.Data?.PlayerName}");
    }

    private IEnumerator CoSendRequestAndWaitForResponseUnderTimeout()
    {
        if (!Player) yield break;
        IsBusy = true;
        VersionHandshakeManager.RpcRequestHandshake(PlayerControl.LocalPlayer, Player);
        while (!HasResponse)
        {
            if (DateTime.UtcNow >= ExpiresAt)
            {
                Specimen.Instance.Log.LogWarning($"HandshakeRequest timeout for player {Player.Data.PlayerName}");
                IsBusy = false;
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
        IsBusy = false;
    }
}