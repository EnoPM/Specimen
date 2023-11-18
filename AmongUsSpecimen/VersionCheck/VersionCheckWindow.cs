using System.Collections.Generic;
using System.Linq;
using AmongUsSpecimen.Options;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.UI.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.VersionCheck;

internal class VersionCheckWindow : UiWindow
{
    public override string Name => "Lobby HUD";
    protected override string Title => $"<b><color=#5925b3>Specimen</color> {Name}</b>";
    public override int MinWidth => 500;
    public override int MinHeight => Screen.height;
    public override bool HasOverlay => false;
    protected override bool DisplayByDefault => false;
    public override bool DisableGameClickWhenOpened => false;
    protected override bool DisplayTitleBar => false;
    public override bool AlwaysOnTop => true;
    protected override Color BackgroundColor => UIPalette.Transparent;
    protected override Positions Position => Positions.MiddleLeft;

    private GameObject _optionsTab;
    private GameObject _playerScrollbar;
    private GameObject _optionsContainer;
    private GameObject _playersContainer;
    private GameObject _menuContainer;
    private AmongUsCheckbox _hudToggle;
    private readonly List<UiOptionTab> _customOptionTabs = new();
    private readonly List<PlayerVersionCheck> _playerVersionChecks = new();
    private bool _hudMenuState = true;
    private SelectedMenus _selectedMenu = SelectedMenus.PlayerVersionHandshakes;
    private GameObject _optionButton;
    private Image _optionButtonImage;
    private GameObject _playerButton;
    private Image _playerButtonImage;
    
    private enum SelectedMenus
    {
        PlayerVersionHandshakes,
        CustomOptions
    }

    protected override void ConstructWindowContent()
    {
        var container = UiFactory.CreateVerticalGroup(ContentRoot, "Container", 
            false, false,
            true, true, 0,
            Vector4.zero, UIPalette.Transparent,
            TextAnchor.LowerLeft);
        container.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(container, MinWidth, MinHeight, 0, 0, 0, 0);

        _optionsTab = UiFactory.CreateVerticalGroup(container, "OptionsTab", false, false, true, true, 0, Vector4.zero, UIPalette.Primary, TextAnchor.UpperLeft);
        _optionsTab.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(_optionsTab, MinWidth, MinHeight - 50, 0, 0, 0, 0);

        var optionsHeader = UiFactory.CreateHorizontalGroup(_optionsTab, "OptionsHeader", false, false, true, true, 2, Vector4.zero, Color.black, TextAnchor.MiddleCenter);
        UiFactory.SetLayoutElement(optionsHeader, MinWidth, 70, 0, 0, 0, 0);
        
        _optionsContainer = UiFactory.CreateVerticalGroup(_optionsTab, "CustomOptionsShower", false, false, true, true);
        _optionsContainer.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(_optionsContainer, MinWidth, MinHeight - 120, 0, 0);
        
        _playerScrollbar = UiFactory.CreateScrollView(container, "PlayerVersionCheckScroller", out _playersContainer, out _, minHeight: MinHeight - 50);
        UiFactory.SetLayoutElement(_playerScrollbar, MinWidth, MinHeight - 50, 0, 0);

        var tabsCount = CustomOptionManager.Tabs.Count + 1;
        for (var i = 0; i < tabsCount; i++)
        {
            if (i == 0)
            {
                _customOptionTabs.Add(new UiVanillaOptionTab(_optionsContainer, MinWidth, MinHeight - 50));
            }
        }

        for (var i = 0; i < 15; i++)
        {
            _playerVersionChecks.Add(new PlayerVersionCheck(CreatePlayerContainer()));
        }

        _menuContainer = UiFactory.CreateHorizontalGroup(container, "PlayerVersionCheckMenuContainer",
                false, false,
                true, true,
                bgColor: Palette.EnabledColor,
                childAlignment: TextAnchor.MiddleLeft,
                spacing: 5,
                padding: new Vector4(0f, 0f, 0f, 0f));
        _menuContainer.GetComponent<Image>().sprite = VersionHandshakeManager.ImageTemplate;
        UiFactory.SetLayoutElement(_menuContainer, MinWidth, 50, 0, 0, 0, 0);

        _hudToggle = new AmongUsCheckbox(_menuContainer, 40, defaultValue: _hudMenuState, maxWidth: 240)
        {
            Label =
            {
                text = "Lobby HUD",
                fontSize = 25,
                fontStyle = FontStyle.Bold,
                color = _hudMenuState ? Color.green : Color.red
            }
        };
        var labelOutline = _hudToggle.Label.gameObject.AddComponent<Outline>();
        labelOutline.effectDistance = new Vector2(1f, 1f);
        labelOutline.effectColor = Color.black;
        _hudToggle.ValueChanged += OnHudToggleValueChanged;

        _optionButton = UiFactory.CreateHorizontalGroup(_menuContainer, "MenuButton", false, false,
            true, true, childAlignment: TextAnchor.MiddleCenter, bgColor: Palette.EnabledColor);
        UiFactory.SetLayoutElement(_optionButton, 120, 40, 0, 0, 0, 0);
        _optionButtonImage = _optionButton.GetComponent<Image>();
        _optionButtonImage.sprite = VersionHandshakeManager.ImageTemplate;
        _optionButtonImage.color = UIPalette.Dark;
        var menuButton = _optionButton.AddComponent<Button>();
        menuButton.onClick.AddListener((UnityAction)(() =>
        {
            _selectedMenu = SelectedMenus.CustomOptions;
            _optionButtonImage.color = UIPalette.Primary;
            _playerButtonImage.color = UIPalette.Dark;
        }));
        var optionButtonText = UiFactory.CreateLabel(_optionButton, "ButtonText", "Options", TextAnchor.MiddleCenter,
            Color.black);
        optionButtonText.fontStyle = FontStyle.Bold;
        var outline = optionButtonText.gameObject.AddComponent<Outline>();
        optionButtonText.fontSize = 24;
        outline.effectDistance = new Vector2(1f, 1f);
        outline.effectColor = Color.white;
        
        _playerButton = UiFactory.CreateHorizontalGroup(_menuContainer, "MenuButton", false, false,
            true, true, childAlignment: TextAnchor.MiddleCenter, bgColor: Palette.EnabledColor);
        UiFactory.SetLayoutElement(_playerButton, 120, 40, 0, 0, 0, 0);
        _playerButtonImage = _playerButton.GetComponent<Image>();
        _playerButtonImage.sprite = VersionHandshakeManager.ImageTemplate;
        _playerButtonImage.color = UIPalette.Primary;
        var menuButton2 = _playerButton.AddComponent<Button>();
        menuButton2.onClick.AddListener((UnityAction)(() =>
        {
            _selectedMenu = SelectedMenus.PlayerVersionHandshakes;
            _optionButtonImage.color = UIPalette.Dark;
            _playerButtonImage.color = UIPalette.Primary;
        }));
        var playerButtonText = UiFactory.CreateLabel(_playerButton, "ButtonText", "Players", TextAnchor.MiddleCenter, Color.black);
        playerButtonText.fontStyle = FontStyle.Bold;
        var outline2 = playerButtonText.gameObject.AddComponent<Outline>();
        playerButtonText.fontSize = 24;
        outline2.effectDistance = new Vector2(1f, 1f);
        outline2.effectColor = Color.white;
        
        _playerScrollbar.SetActive(false);
        _optionsTab.SetActive(false);
    }

    private void OnHudToggleValueChanged(bool value)
    {
        _hudMenuState = value;
        _hudToggle.Label.color = _hudMenuState ? Color.green : Color.red;
        _optionButton.SetActive(_hudMenuState);
        _playerButton.SetActive(_hudMenuState);
        _menuContainer.GetComponent<Image>().enabled = _hudMenuState;
    }

    internal void RefreshStates()
    {
        if (!PlayerControl.LocalPlayer || PlayerControl.AllPlayerControls == null) return;
        if (!_hudMenuState)
        {
            _playerScrollbar.SetActive(false);
            _optionsTab.SetActive(false);
            return;
        }

        if (_selectedMenu == SelectedMenus.CustomOptions)
        {
            _playerScrollbar.SetActive(false);
            _optionsTab.SetActive(true);
            foreach (var tab in _customOptionTabs)
            {
                tab.UiUpdate();
            }
            return;
        }
        _optionsTab.SetActive(false);
        _playerScrollbar.SetActive(true);
        var sortedPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => x.Data != null)
            .OrderBy(x => x.Data.PlayerName).ToArray();
        for (var i = 0; i < _playerVersionChecks.Count; i++)
        {
            var element = _playerVersionChecks[i];
            var player = sortedPlayers.Length > i ? sortedPlayers[i] : null;
            // var player = sortedPlayers.Length > 0 && i < 10 ? sortedPlayers[0] : null;
            element.Refresh(player);
        }
    }

    internal bool ShouldBlockGameStart()
    {
        return _playerVersionChecks.Any(x => x.ShouldBlockGameStart);
    }

    private GameObject CreatePlayerContainer()
    {
        var obj = UiFactory.CreateHorizontalGroup(
            _playersContainer,
            "PlayerContainer",
            false,
            false,
            true,
            true,
            0,
            Vector4.zero,
            UIPalette.Transparent,
            TextAnchor.UpperLeft
        );
        obj.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(obj, PlayerVersionCheck.Width, PlayerVersionCheck.Height, 0, 0);
        obj.SetActive(false);
        return obj;
    }
}