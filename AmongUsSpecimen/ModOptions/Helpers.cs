using UnityEngine;

namespace AmongUsSpecimen.ModOptions;

public static class Helpers
{
    public static TOption Local<TOption>(TOption option) where TOption : BaseModOption
    {
        option.SetRestriction(OptionRestriction.Local)
            .SetSaveLocation(OptionSaveLocation.Local);
        return option;
    }
    
    public static TOption OutsidePreset<TOption>(TOption option) where TOption : BaseModOption
    {
        option.SetSaveLocation(OptionSaveLocation.Global);
        return option;
    }
    
    public static TOption Inverted<TOption>(TOption option) where TOption : BaseModOption
    {
        option.SetEnabledIfParentDisabled(true);
        return option;
    }
    
    public static TOption Header<TOption>(TOption option) where TOption : BaseModOption
    {
        option.SetIsHeader(true);
        return option;
    }

    public static void OverrideSprite(this ModOptionTab tab, Sprite sprite)
    {
        tab.IconSprite = sprite;
        if (tab.Highlight)
        {
            tab.Highlight.sprite = tab.IconSprite;
        }

        if (tab.UiHighlight)
        {
            tab.UiHighlight.sprite = tab.IconSprite;
        }
    }
    
    public static int BeforeTab(ModOptionTab tab)
    {
        return ModOptionManager.Tabs.IndexOf(tab);
    }
    
    public static int AfterTab(ModOptionTab tab)
    {
        return ModOptionManager.Tabs.IndexOf(tab) + 1;
    }
}