using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AmongUsSpecimen.ModOptions;

public class ModOptionTab
{
    public string Key { get; set; }
    public string Title { get; set; }
    public Sprite IconSprite { get; set; }
    public IEnumerable<BaseModOption> Options => ModOptionManager.Options.Where(x => x.Tab.Key == Key);
    
    public TextMeshPro TitleTMP { get; internal set; }
    internal GameObject TabObject { get; set; }
    internal GameObject TabPositionObject { get; set; }
    internal GameObject UiTabObject { get; set; }
    internal SpriteRenderer Highlight { get; set; }
    internal GameObject SettingsGameObject { get; set; }
    internal Image UiHighlight { get; set; }

    public bool IsActive { get; private set; } = true;
    
    public ModOptionTab(string key, string title, Sprite iconSprite, int at = -2)
    {
        Key = key;
        Title = title;
        IconSprite = iconSprite;
        if (at == -2)
        {
            ModOptionManager.Tabs.Add(this);
        }
        else
        {
            ModOptionManager.Tabs.Insert(at, this);
        }
    }

    public void SetActive(bool active)
    {
        var wasActive = IsActive;
        
        if (wasActive == active) return;
        IsActive = active;
        if (UiTabObject)
        {
            UiTabObject.SetActive(IsActive);
        }
        ModOptionManager.RecreateCustomTabs();
    }
    
    private const float BaseOptionsPerRow = 8f;
    private static float _timer = 1f;

    internal void BehaviourUpdate(GameOptionsMenu optionsMenu)
    {
        var scroller = optionsMenu.GetComponentInParent<Scroller>();
        var options = Options.ToList();
        var optionsCount = (float)options.Count(x => x.IsParentEnabled);
        var headerOptions = (float)options.Count(x => x.IsParentEnabled && x.IsHeader);
        scroller.ContentYBounds.max = -BaseModOption.BaseOptionYOffset + (optionsCount - (BaseOptionsPerRow - headerOptions / optionsCount)) * BaseModOption.BaseOptionYOffset + headerOptions * BaseModOption.AdditionalHeaderYOffset;
        scroller.UpdateScrollBars();
            
        _timer += Time.deltaTime;
        if (_timer < 0.1f) return;
        _timer = 0f;

        var offset = 2.75f;
        foreach (var option in options)
        {
            if (option.OptionBehaviour == null || option.OptionBehaviour.gameObject == null) continue;
            option.BehaviourUpdate();
            if (!option.IsParentEnabled) continue;
            offset -= option.IsHeader ? BaseModOption.BaseOptionYOffset + BaseModOption.AdditionalHeaderYOffset : BaseModOption.BaseOptionYOffset;
            var transform = option.OptionBehaviour.transform;
            var localPosition = transform.localPosition;
            transform.localPosition = new Vector3(localPosition.x, offset, localPosition.z);
        }
    }
}