using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using AmongUsSpecimen.ModOptions;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.UI.LobbyHud;
using AmongUsSpecimen.Utils;
using InnerNet;
using UnityEngine;

namespace AmongUsSpecimen.VersionCheck;

public static class VersionHandshakeManager
{
    internal static readonly Dictionary<int, VersionHandshake> AllHandshakes = new();
    internal static readonly VersionHandshake LocalHandshake = new()
    {
        Flags = new Dictionary<string, string>(),
        Mods = new Dictionary<string, VersionHandshake.ModVersion>()
    };

    internal static Sprite ImageTemplate => SpecimenSprites.UiBackgroundBase;
    internal static Sprite SuccessIcon => SpecimenSprites.SuccessIcon;
    internal static Sprite WarningIcon => SpecimenSprites.WarningIcon;
    internal static Sprite ErrorIcon => SpecimenSprites.ErrorIcon;

    internal static bool ShouldBlockGameStart => Window?.PlayersBehaviour.ShouldBlockGameStart() == true;
    
    internal static PlayerControl HostPlayerControl =>
        PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data != null && x.OwnerId == AmongUsClient.Instance.HostId);
    
    internal static LobbyHudWindow Window { get; private set; }

    internal static void Start()
    {
        Window = UiManager.RegisterWindow<LobbyHudWindow>();
        Window.SetActive(false);
    }

    internal static bool TryGetHandshake(PlayerControl playerControl, out VersionHandshake handshake)
    {
        if (!playerControl)
        {
            handshake = null;
            return false;
        }
        if (!playerControl.AmOwner) return AllHandshakes.TryGetValue(playerControl.OwnerId, out handshake);
        handshake = LocalHandshake;
        return true;

    }

    internal static void RegisterAssembly(Assembly assembly)
    {
        var types = assembly.GetClassesByAttribute<VersionHandshakeAttribute>();
        foreach (var type in types)
        {
            var attribute = type.Attribute;
            LocalHandshake.Mods[attribute.Name] = new VersionHandshake.ModVersion
            {
                Guid = assembly.ManifestModule.ModuleVersionId,
                Version = Version.Parse(attribute.Version),
                CheckGuid = attribute.CheckGuid
            };
        }
    }
    
    [Rpc(LocalExecution.None)]
    private static void RpcSendHandshake(PlayerControl __sender, PlayerControl __receiver, VersionHandshake handshake)
    {
        AllHandshakes[handshake.ClientId] = handshake;
        if (_handshakeRequests.TryGetValue(handshake.ClientId, out var request))
        {
            request.TriggerResponse();
        }
    }

    [Rpc(LocalExecution.None)]
    internal static void RpcRequestHandshake(PlayerControl __sender, PlayerControl __receiver)
    {
        RpcSendHandshake(PlayerControl.LocalPlayer, __sender, LocalHandshake);
        ModOptionManager.SendCurrentPresetTo(__sender);
    }

    private static readonly Dictionary<int, HandshakeRequest> _handshakeRequests = new();
    
    internal static void AskForHandshake(PlayerControl player)
    {
        var ownerId = player.OwnerId;
        if (!_handshakeRequests.TryGetValue(ownerId, out var request))
        {
            request = _handshakeRequests[ownerId] = new HandshakeRequest();
        }
        if (AllHandshakes.ContainsKey(ownerId)) return;
        request.Run(player);
    }
}