using System;
using AmongUsSpecimen.UI.Components;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.LobbyHud;

internal class LobbyHudWindow : UiWindow
{
    public const int Height = 50;
    
    public override string Name => "Lobby HUD";
    public override int MinWidth => 500;
    public override int MinHeight => 50;
    public override bool HasOverlay => false;
    protected override bool DisplayByDefault => false;
    public override bool DisableGameClickWhenOpened => false;
    protected override bool DisplayTitleBar => false;
    public override bool AlwaysOnTop => true;
    protected override Color BackgroundColor => UIPalette.Transparent;
    protected override Positions Position => Positions.BottomLeft;

    internal bool ShouldBeDisplayed { get; set; } = true;
    private GameObject _container;
    internal AmongUsCheckbox HudToggle;

    private MenuButton _optionsMenu;
    private MenuButton _playersMenu;

    internal OptionsHudMenuBehaviour OptionsBehaviour;
    internal PlayersHudMenuBehaviour PlayersBehaviour;

    private Menus _currentMenu = Menus.Players;
    
    protected override void ConstructWindowContent()
    {
        OptionsBehaviour = Specimen.Instance.AddComponent<OptionsHudMenuBehaviour>();
        PlayersBehaviour = Specimen.Instance.AddComponent<PlayersHudMenuBehaviour>();
        
        _container = UiFactory.CreateHorizontalGroup(ContentRoot, nameof(LobbyHudWindow),
            false, false, true, true,
            5, bgColor: Palette.EnabledColor, padding: new Vector4(3f, 0f, 0f, 0f));
        UiFactory.SetLayoutElement(_container, MinWidth, MinHeight, 0, 0, 0, 0);
        _container.GetComponent<Image>().sprite = SpecimenSprites.UiBackgroundBase;
        HudToggle = new AmongUsCheckbox(_container, 40, defaultValue: ShouldBeDisplayed, maxWidth: 235)
        {
            Label =
            {
                text = "Lobby HUD",
                fontSize = 25,
                fontStyle = FontStyle.Bold,
                color = ShouldBeDisplayed ? Color.green : Color.red
            }
        };
        var labelOutline = HudToggle.Label.gameObject.AddComponent<Outline>();
        labelOutline.effectDistance = new Vector2(1f, 1f);
        labelOutline.effectColor = Color.black;
        HudToggle.ValueChanged += OnHudToggleValueChanged;

        _optionsMenu = CreateMenuButton("Options", OnOptionsButtonClick);
        _playersMenu = CreateMenuButton("Players", OnPlayersButtonClick);
    }

    private void OnOptionsButtonClick()
    {
        _currentMenu = Menus.Options;
        UpdateMenus();
    }

    private void OnPlayersButtonClick()
    {
        _currentMenu = Menus.Players;
        UpdateMenus();
    }
    
    private void OnHudToggleValueChanged(bool value)
    {
        ShouldBeDisplayed = value;
        RefreshState();
    }

    private void RefreshState()
    {
        HudToggle.Label.color = ShouldBeDisplayed ? Color.green : Color.red;
        _optionsMenu.GameObject.SetActive(ShouldBeDisplayed);
        _playersMenu.GameObject.SetActive(ShouldBeDisplayed);
        _container.GetComponent<Image>().enabled = ShouldBeDisplayed;
        UpdateMenus();
    }

    private void UpdateMenus()
    {
        if (!PlayersBehaviour || !OptionsBehaviour || _optionsMenu == null || _playersMenu == null) return;
        if (!ShouldBeDisplayed || !Enabled)
        {
            PlayersBehaviour.SetActive(false);
            OptionsBehaviour.SetActive(false);
            _optionsMenu.Image.color = UIPalette.Dark;
            _playersMenu.Image.color = UIPalette.Dark;
            return;
        }
        if (_currentMenu == Menus.Players)
        {
            _optionsMenu.Image.color = UIPalette.Dark;
            _playersMenu.Image.color = UIPalette.Success;
            OptionsBehaviour.SetActive(false);
            PlayersBehaviour.SetActive(true);
        }
        else if (_currentMenu == Menus.Options)
        {
            _optionsMenu.Image.color = UIPalette.Success;
            _playersMenu.Image.color = UIPalette.Dark;
            PlayersBehaviour.SetActive(false);
            OptionsBehaviour.SetActive(true);
        }
        else
        {
            _optionsMenu.Image.color = UIPalette.Dark;
            _playersMenu.Image.color = UIPalette.Dark;
            PlayersBehaviour.SetActive(false);
            OptionsBehaviour.SetActive(false);
        }
    }

    private MenuButton CreateMenuButton(string text, Action onClick)
    {
        var buttonContainer = UiFactory.CreateHorizontalGroup(_container, "MenuButton", false, false, true, true, childAlignment: TextAnchor.MiddleCenter, bgColor: UIPalette.Dark);
        UiFactory.SetLayoutElement(buttonContainer, 120, 40, 0, 0, 0, 0);
        
        var image = buttonContainer.GetComponent<Image>();
        image.sprite = SpecimenSprites.UiBackgroundBase;
        
        var label = UiFactory.CreateLabel(buttonContainer, "ButtonLabel", text, TextAnchor.MiddleCenter, Color.black,
            true, 24);
        label.fontStyle = FontStyle.Bold;

        var outline = label.gameObject.AddComponent<Outline>();
        
        var button = buttonContainer.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        outline.effectDistance = new Vector2(1f, 1f);
        outline.effectColor = Color.white;

        return new MenuButton(buttonContainer, image, label, outline, button);
    }
    
    private enum Menus
    {
        Options,
        Players
    }
    
    private class MenuButton
    {
        internal readonly GameObject GameObject;
        internal readonly Image Image;
        internal readonly Text Label;
        internal readonly Outline LabelOutline;
        internal readonly Button Button;

        internal MenuButton(GameObject gameObject, Image image, Text label, Outline labelOutline, Button button)
        {
            GameObject = gameObject;
            Image = image;
            Label = label;
            LabelOutline = labelOutline;
            Button = button;
        }
    }

    public override void SetActive(bool active)
    {
        base.SetActive(active);
        UpdateMenus();
    }
}