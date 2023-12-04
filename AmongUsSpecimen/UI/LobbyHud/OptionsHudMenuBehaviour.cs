using System;
using AmongUsSpecimen.Utils;
using UnityEngine;

namespace AmongUsSpecimen.UI.LobbyHud;

[RegisterMonoBehaviour]
internal class OptionsHudMenuBehaviour(IntPtr ptr) : MonoBehaviour(ptr)
{
    
    internal OptionsHudWindow Window { get; set; }
    private int _hostIdCache { get; set; }

    internal void Start()
    {
        Window = UiManager.RegisterWindow<OptionsHudWindow>();
        _hostIdCache = AmongUsClient.Instance ? AmongUsClient.Instance.HostId : -1;
    }

    internal void Update()
    {
        Window.Refresh();
        RefreshHostId();
    }

    private void RefreshHostId()
    {
        var hostId = AmongUsClient.Instance ? AmongUsClient.Instance.HostId : -1;
        if (hostId != _hostIdCache)
        {
            _hostIdCache = hostId;
            GameEventManager.TriggerHostChanged();
        }
    }

    internal void SetActive(bool value) => Window?.SetActive(value);
}