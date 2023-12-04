using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AmongUsSpecimen.Extensions;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.UI.Components;
using AmongUsSpecimen.Utils;
using AmongUsSpecimen.Utils.Converters;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.VersionCheck;

internal class PlayerVersionCheck
{
    internal const int Width = 300;
    internal const int Height = 60;
    
    internal GameObject ContentRoot;
    internal bool ShouldBlockGameStart { get; private set; }

    private readonly Text _playerName;
    private readonly Outline _playerNameOutline;
    private readonly List<ModVersionCheck> _mods = new();
    private readonly List<PlayerBadge> _badges = new();
    private readonly PlayerCharacter _character;
    private readonly GameObject _modsContainer;
    private readonly Image _modContainerBackground;
    private readonly Image _background;
    private readonly Image _icon;
    private bool _isExpanded;

    internal PlayerVersionCheck(GameObject root)
    {
        ContentRoot = root;

        var master = UiFactory.CreateVerticalGroup(ContentRoot, "PlayerVersionCheckMaster", false, false, true, true,
            0, Vector4.zero, UIPalette.Transparent);
        master.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(master, Width, Height, 0, 0);
        
        var main = UiFactory.CreateHorizontalGroup(master, "PlayerVersionCheckMain", false, false, true, true, 
            5, Vector4.zero, Palette.EnabledColor, TextAnchor.MiddleCenter);
        _background = main.GetComponent<Image>();
        UiFactory.SetLayoutElement(main, Width, Height, 0, 0, 0, 0);
        
        _modsContainer = UiFactory.CreateVerticalGroup(master, "ModsContainer",
        false, false, true, true, 0,
        new Vector4(5f, 5f, 5f, 5f), Palette.EnabledColor);
        _modContainerBackground = _modsContainer.GetComponent<Image>();
        UiFactory.SetLayoutElement(_modsContainer, Width, 0, 0, 0, preferredWidth: 0);
        
        _character = new PlayerCharacter(main);
        
        var container = UiFactory.CreateVerticalGroup(main, "Container", false, false, true, true,
            bgColor: UIPalette.LightDanger, childAlignment: TextAnchor.UpperLeft, padding: new Vector4(5f, 5f, 5f, 5f), spacing: 5);
        UiFactory.SetLayoutElement(container, Width - 115, Height, 0, 0, 0, 0);
        container.GetComponent<Image>().enabled = false;

        var iconContainer = UiFactory.CreateVerticalGroup(main, "IconContainer", false, false, true, true,
            bgColor: Palette.EnabledColor, childAlignment: TextAnchor.MiddleCenter, padding: new Vector4(0f, 0f, 0f, 0f), spacing: 0);
        UiFactory.SetLayoutElement(iconContainer, 50, 50, 0, 0, 0, 0);
        _icon = iconContainer.GetComponent<Image>();
        
        _playerName = UiFactory.CreateLabel(container, "PlayerName", string.Empty, color: Color.white, fontSize: 18, alignment: TextAnchor.MiddleLeft);
        _playerName.fontStyle = FontStyle.Bold;
        UiFactory.SetLayoutElement(_playerName.gameObject, Width - 5, 20, 0, 0);

        var badgeContainer = UiFactory.CreateHorizontalGroup(container, "BadgeContainer", false, false, true, true, 2, Vector4.zero, Palette.EnabledColor, TextAnchor.MiddleLeft);
        badgeContainer.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(badgeContainer, Width - 120, 20, 0, 0, 0, 0);
        const int badgeWidth = (Width - 120) / 3;
        for (var i = 0; i < 3; i++)
        {
            _badges.Add(new PlayerBadge(badgeContainer, badgeWidth, 20));
        }

        _playerNameOutline = _playerName.gameObject.AddComponent<Outline>();
        _playerNameOutline.effectColor = Color.black;
        _playerNameOutline.effectDistance = Vector2.one;
        
        for (var i = 0; i < 3; i++)
        {
            _mods.Add(new ModVersionCheck(_modsContainer, Color.red, "No handshake"));
        }

        var button = main.AddComponent<Button>();
        button.onClick.AddListener((UnityAction) ToggleExpanded);
    }

    private void ToggleExpanded()
    {
        _isExpanded = !_isExpanded;
    }

    private void SetPlayerName(string playerName, Color? color = null, Color? outlineColor = null)
    {
        _playerName.text = playerName;
        if (color.HasValue)
        {
            _playerName.color = color.Value;
        }

        if (outlineColor.HasValue)
        {
            _playerNameOutline.effectColor = outlineColor.Value;
        }
    }

    private DateTime _nextCheck = DateTime.UtcNow;
    private bool _isSettingBackground;
    private string _currentNameplateId = string.Empty;

    private IEnumerator CoSetBackground(PlayerControl player)
    {
        if (_isSettingBackground) yield break;
        _isSettingBackground = true;
        if (!player || player.Data?.DefaultOutfit?.NamePlateId == null)
        {
            _isSettingBackground = false;
            yield break;
        }

        var nameplateId = player.Data.DefaultOutfit.NamePlateId;
        var nameplate = DestroyableSingleton<HatManager>.Instance.GetNamePlateById(nameplateId);
        var asset = nameplate.CreateAddressableAsset();
        yield return asset.CoLoadAsync();
        var resource = asset.GetAsset();
        _background.sprite = resource.Image;
        _currentNameplateId = nameplateId;
        _isSettingBackground = false;
        _modContainerBackground.sprite = VersionHandshakeManager.ImageTemplate;
    }

    internal void Refresh(PlayerControl player)
    {
        _modsContainer.SetActive(_isExpanded);
        if (DateTime.UtcNow < _nextCheck) return;
        _nextCheck = DateTime.UtcNow.Add(TimeSpan.FromSeconds(0.5));
        if (!player || !AmongUsClient.Instance || AmongUsClient.Instance.HostId < 0 || player.OwnerId < 0)
        {
            ContentRoot.SetActive(false);
            return;
        }
        ContentRoot.SetActive(true);
        _character.SetPlayer(player);
        if (!_isSettingBackground && player.Data.DefaultOutfit.NamePlateId != _currentNameplateId)
        {
            UiManager.Behaviour.StartCoroutine(CoSetBackground(player));
        }
        SetPlayerName(player.Data.PlayerName, Color.white);
        var isOk = true;
        var hasHandshake = VersionHandshakeManager.TryGetHandshake(player, out var handshake);
        var hasHostHandshake = VersionHandshakeManager.TryGetHandshake(VersionHandshakeManager.HostPlayerControl, out var hostHandshake);
        var sortedModKeys = hasHostHandshake ? hostHandshake.Mods.Keys.ToList() : new List<string>();
        if (hasHostHandshake)
        {
            sortedModKeys.AddRange(hostHandshake.Mods.Keys.ToList());
            sortedModKeys = sortedModKeys.Deduplicate((a, b) => a == b);
        }

        if (hasHandshake)
        {
            var hasBadges = handshake.Flags.TryGetValue("badges", out var rawBadges);
            var badges = hasBadges ? JsonSerializer.Deserialize<List<HandshakeBadge>>(rawBadges) : new List<HandshakeBadge>();
            
            for (var i = 0; i < _badges.Count; i++)
            {
                var badge = _badges[i];
                if (i == 0 && AmongUsClient.Instance.HostId == player.OwnerId)
                {
                    badge.GameObject.SetActive(true);
                    badge.Text.text = "Host";
                    badge.Text.color = Color.white;
                    badge.Background.color = Color.red;
                    continue;
                }

                if (!hasBadges)
                {
                    badge.GameObject.SetActive(false);
                    continue;
                }

                var index = i - 1;
                if (badges.Count <= index)
                {
                    badge.GameObject.SetActive(false);
                    continue;
                }
                var api = badges[index];
                badge.GameObject.SetActive(true);
                badge.Text.text = api.Text;
                badge.Text.color = api.TextColor;
                badge.Background.color = api.BackgroundColor;
            }
        }

        sortedModKeys = sortedModKeys.OrderBy(x => x).ToList();
        for (var i = 0; i < _mods.Count; i++)
        {
            var element = _mods[i];
            if (!hasHandshake)
            {
                isOk = false;
                element.SetTextAndIcon("No handshake", Color.blue, VersionHandshakeManager.ErrorIcon);
                element.SetActive(i == 0);
                if (player.OwnerId > 0)
                {
                    VersionHandshakeManager.AskForHandshake(player);
                }
                continue;
            }

            var modKey = sortedModKeys.Count > i ? sortedModKeys[i] : null;
            if (modKey == null)
            {
                element.SetActive(false);
                continue;
            }
            element.SetActive(true);
            var hasMod = handshake.Mods.TryGetValue(modKey, out var mod);
            VersionHandshake.ModVersion hostMod = null;
            var hasHostMod = hasHostHandshake && hostHandshake.Mods.TryGetValue(modKey, out hostMod);
            if (!hasMod && !hasHostMod)
            {
                element.SetActive(false);
                continue;
            }
            if (!hasHostMod)
            {
                element.SetTextAndIcon($"{modKey} v{mod?.Version}", Color.grey, VersionHandshakeManager.WarningIcon);
                continue;
            }

            if (!hasMod)
            {
                element.SetTextAndIcon($"<b>{modKey}</b> {ColorHelpers.Colorize(Color.red, $"v{hostMod.Version}")}", Color.black, VersionHandshakeManager.ErrorIcon);
                isOk = false;
                continue;
            }

            if (hostMod.Version.Equals(mod.Version))
            {
                
                if (hostMod.CheckGuid && !hostMod.Guid.Equals(mod.Guid))
                {
                    element.SetTextAndIcon($"{modKey} {ColorHelpers.Colorize(UIPalette.Success, $"v{mod.Version}")}", Color.black, VersionHandshakeManager.ErrorIcon);
                    isOk = false;
                    continue;
                }

                element.SetTextAndIcon($"{modKey} {ColorHelpers.Colorize(UIPalette.Success, $"v{mod.Version}")}", Color.black, VersionHandshakeManager.SuccessIcon);
                
                continue;
            }
            
            if (hostMod.Version.CompareTo(mod.Version) > 0)
            {
                element.SetTextAndIcon($"{modKey} {ColorHelpers.Colorize(UIPalette.Warning * 1.5f, $"v{mod.Version}")} < {ColorHelpers.Colorize(UIPalette.Success, $"v{hostMod.Version}")}", Color.black, VersionHandshakeManager.ErrorIcon);
                isOk = false;
                continue;
            }
            
            element.SetTextAndIcon($"{modKey} {ColorHelpers.Colorize(UIPalette.Success, $"v{mod.Version}")} > {ColorHelpers.Colorize(UIPalette.Warning * 1.5f, $"v{hostMod.Version}")}", Color.black, VersionHandshakeManager.ErrorIcon);
            isOk = false;
        }
        
        _icon.sprite = isOk ? VersionHandshakeManager.SuccessIcon : VersionHandshakeManager.ErrorIcon;
        ShouldBlockGameStart = !isOk;
    }
}

internal class PlayerBadge
{
    internal readonly GameObject GameObject;
    internal readonly Image Background;
    internal readonly Text Text;

    internal PlayerBadge(GameObject parent, int width, int height)
    {
        GameObject = UiFactory.CreateHorizontalGroup(parent, "PlayerBadge", false, false, true, true, bgColor: Color.white, childAlignment: TextAnchor.MiddleCenter);
        UiFactory.SetLayoutElement(GameObject, width, height, 0, 0, 0, 0);
        Background = GameObject.GetComponent<Image>();
        Text = UiFactory.CreateLabel(GameObject, "Text", string.Empty, TextAnchor.MiddleCenter, Color.black, true, 15);
        Text.fontStyle = FontStyle.Bold;
    }
}

public class HandshakeBadge
{
    public string Text { get; set; }
    
    [JsonConverter(typeof(UnityColorConverter))]
    public Color TextColor { get; set; }
    
    [JsonConverter(typeof(UnityColorConverter))]
    public Color BackgroundColor { get; set; }
}