using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUsSpecimen.ModOptions;
using AmongUsSpecimen.Utils;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.LobbyHud;

public class OptionsHudWindow : UiWindow
{
    internal const int VanillaTabCount = 1;
    
    public override string Name => "Options HUD";
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
    
    internal readonly List<UiOptionTab> OptionTabs = [];
    private int _currentTabIndex = 0;
    internal UiOptionTab CurrentTab => OptionTabs[_currentTabIndex];
    
    protected override void ConstructWindowContent()
    {
        var buttonsContainer = UiFactory.CreateHorizontalGroup(ContentRoot, "ButtonsContainer", false, false, true,
            true, 5, bgColor: UIPalette.Dark, childAlignment: TextAnchor.MiddleCenter);
        UiFactory.SetLayoutElement(buttonsContainer, MinWidth, 70, 0, 0, 0, 0);
        var tabsContainer = UiFactory.CreateHorizontalGroup(ContentRoot, "TabsContainer", false, false, true, true);
        tabsContainer.GetComponent<Image>().enabled = false;
        UiFactory.SetLayoutElement(tabsContainer, MinWidth, MinHeight - 70, 0, 0, 0, 0);
        var tabsCount = ModOptionUtility.Tabs.Count + VanillaTabCount;
        var allTabButtons = new List<Image>();
        for (var i = 0; i < tabsCount; i++)
        {
            var item = UiFactory.CreateVerticalGroup(buttonsContainer, "TabButton", false, false, true, true,
                childAlignment: TextAnchor.MiddleCenter);
            UiFactory.SetLayoutElement(item, 70, 57, 0, 0, 0, 0);
            var image = item.GetComponent<Image>();
            image.color = i == _currentTabIndex ? Palette.EnabledColor : Palette.DisabledClear;
            if (i < VanillaTabCount)
            {
                if (i == 0)
                {
                    image.sprite = Assembly.GetExecutingAssembly().LoadSpriteFromResources("AmongUsSpecimen.Resources.Sprites.VanillaSettingsTabIcon.png", 400f);
                }
            }
            else
            {
                var customTab = ModOptionUtility.Tabs[i - VanillaTabCount];
                customTab.UiTabObject = item;
                image.sprite = customTab.IconSprite;
                customTab.UiHighlight = image;
                item.SetActive(customTab.IsActive);

            }

            var button = item.AddComponent<Button>();
            var currentIndex = i;
            button.onClick.AddListener((Action)(() =>
            {
                _currentTabIndex = currentIndex;
                foreach (var img in allTabButtons)
                {
                    img.color = Palette.DisabledClear;
                }

                allTabButtons[_currentTabIndex].color = Palette.EnabledColor;
            }));
            allTabButtons.Add(image);
        }
        for (var i = 0; i < tabsCount; i++)
        {
            if (i < VanillaTabCount)
            {
                if (i == 0)
                {
                    OptionTabs.Add(new UiVanillaOptionTab(tabsContainer, MinWidth, MinHeight - 70));
                }
                continue;
            }

            var index = i - VanillaTabCount;
            var tab = new UiCustomOptionTab(tabsContainer, MinWidth, MinHeight - 70, ModOptionUtility.Tabs[index]);
            OptionTabs.Add(tab);
        }
    }

    internal void Refresh()
    {
        for (var i = 0; i < OptionTabs.Count; i++)
        {
            var tab = OptionTabs[i];
            var active = i == _currentTabIndex;
            tab.SetActive(active);
            if (active)
            {
                tab.UiUpdate();
            }
        }
    }
}