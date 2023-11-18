using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace AmongUsSpecimen.Options;

public static class CustomOptionManager
{
    public const float MinCommonTaskCount = 0f;
    public const float MaxCommonTaskCount = 4f;
    public const float MinShortTaskCount = 0f;
    public const float MaxShortTaskCount = 23f;
    public const float MinLongTaskCount = 0f;
    public const float MaxLongTaskCount = 15f;

    internal static readonly List<CustomOptionTab> Tabs = new();
    internal static readonly List<CustomOption> Options = new();

    public static readonly CustomOptionPreset OnlinePreset = new()
    {
        IsSharable = true,
        Name = "Online",
        Values = new Dictionary<int, int>()
    };

    private static CustomOptionPreset _currentPreset { get; set; } = new()
    {
        IsSharable = true,
        Name = "Default",
        Values = new Dictionary<int, int>()
    };

    internal static CustomOptionPreset CurrentPreset
    {
        get
        {
            if (PlayerConditions.AmHostOrNotInGame())
            {
                return _currentPreset;
            }

            return OnlinePreset;
        }
        set
        {
            if (PlayerConditions.AmHostOrNotInGame())
            {
                _currentPreset = value;
            }
            foreach (var (k, v) in value.Values)
            {
                OnlinePreset.SetValue(k, v, true);
            }
        }
    }

    [Rpc]
    public static void RpcSetCurrentPreset(CustomOptionPreset preset)
    {
        CurrentPreset = preset;
    }

    [Rpc]
    public static void RpcSetValue(int key, int value)
    {
        SetValue_Internal(key, value);
    }

    internal static void RegisterOption(CustomOption option)
    {
        Options.Add(option);
        Options.Sort(CompareOption);
    }

    internal static void SetOptionsValues()
    {
        foreach (var option in Options)
        {
            if (CurrentPreset.Values.TryGetValue(option.Id, out var value))
            {
                option.SetCurrentSelection_Internal(value);
            }
            else
            {
                CurrentPreset.SetValue(option.Id, value);
            }
        }
    }
    
    private static int CompareOption(CustomOption a, CustomOption b)
    {
        return string.Compare(a.Name, b.Name, StringComparison.InvariantCulture);
    }
    
    internal static void SetValue_Internal(int k, int v)
    {
        CurrentPreset.SetValue(k, v, !PlayerConditions.AmHostOrNotInGame());
    }

    private static void SetValue(this CustomOptionPreset preset, int key, int value, bool updateCustomOption = false)
    {
        preset.Values[key] = value;
        if (!updateCustomOption) return;
        var option = key >= 0 && key < Options.Count ? Options[key] : null;
        if (option == null)
        {
            Specimen.Instance.Log.LogWarning($"[{nameof(CustomOptionManager)}] Trying to set value of non-existing custom option");
            return;
        }
        if (preset.Name != CurrentPreset.Name) return;
        option.SetCurrentSelection_Internal(value);
    }

    private static bool TryGetOption(OptionBehaviour optionBehaviour, out CustomOption customOption)
    {
        customOption = Options.Find(x => x.OptionBehaviour == optionBehaviour);
        if (customOption != null)
        {
            optionBehaviour.OnValueChanged = (Action<OptionBehaviour>)EmptyAction;
        }
        return customOption != null;
    }

    internal static bool CustomOptionEnable(StringOption stringOption)
    {
        if (!TryGetOption(stringOption, out var option)) return true;
        stringOption.TitleText.text = option.Name;
        stringOption.Value = stringOption.oldValue = option.CurrentSelection;
        stringOption.ValueText.text = option.DisplayValue;
        return false;
    }

    internal static bool CustomOptionEnable(ToggleOption boolOption)
    {
        if (!TryGetOption(boolOption, out var option)) return true;
        boolOption.TitleText.text = option.Name;
        boolOption.CheckMark.enabled = boolOption.oldValue = option.CurrentSelection > 0;
        return false;
    }

    internal static bool CustomOptionIncrease(OptionBehaviour stringOption)
    {
        if (!TryGetOption(stringOption, out var option)) return true;
        option.CurrentSelection += 1;
        return false;
    }
    
    internal static bool CustomOptionDecrease(OptionBehaviour stringOption)
    {
        if (!TryGetOption(stringOption, out var option)) return true;
        option.CurrentSelection -= 1;
        return false;
    }
    
    private static float _timer = 1f;

    internal static void CustomOptionMenuUpdate(GameOptionsMenu optionsMenu)
    {
        var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
        if (gameSettingMenu != null && (gameSettingMenu.RegularGameSettings.active ||
                                        gameSettingMenu.RolesSettings.gameObject.active)) return;
        optionsMenu.GetComponentInParent<Scroller>().ContentYBounds.max = -0.5F + optionsMenu.Children.Length * 0.55F;
        _timer += Time.deltaTime;
        if (_timer < 0.1f) return;
        _timer = 0f;

        foreach (var tab in Tabs)
        {
            var offset = 2.75f;
            foreach (var option in Options.Where(x => x.Tab == tab))
            {
                if (option.OptionBehaviour == null || option.OptionBehaviour.gameObject == null) continue;
                var enabled = true;
                var parent = option.Parent;
                while (parent != null && enabled)
                {
                    enabled = parent.CurrentSelection > 0;
                    parent = parent.Parent;
                }

                option.OptionBehaviour.gameObject.SetActive(enabled);
                if (!enabled) continue;
                offset -= option.IsHeader ? 0.75f : 0.5f;
                var transform = option.OptionBehaviour.transform;
                var localPosition = transform.localPosition;
                transform.localPosition = new Vector3(localPosition.x, offset, localPosition.z);
            }
            
        }
    }

    private static void EmptyAction(OptionBehaviour _)
    {
    }
    
    internal static void CreateCustomOptionTabs(GameOptionsMenu gameOptionsMenu)
    {
        if (TryRefreshNames()) return;
        var stringTemplate = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
        var boolTemplate = UnityEngine.Object.FindObjectsOfType<ToggleOption>().FirstOrDefault();
        if (stringTemplate == null || boolTemplate == null) return;

        var gameSettings = GameObject.Find("Game Settings");
        var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
        if (gameSettingMenu == null) return;
        
        var customSettings = new Dictionary<string, GameObject>();
        var customMenus = new Dictionary<string, GameOptionsMenu>();
        var customTabs = new Dictionary<string, GameObject>();
        var customTabHighlights = new Dictionary<string, SpriteRenderer>();
        
        var roleTab = GameObject.Find("RoleTab");
        var gameTab = GameObject.Find("GameTab");
        
        for (var index = 0; index < Tabs.Count; index++)
        {
            var tabInfo = Tabs[index];
            GameObject setting;
            GameObject tab;
            if (index == 0)
            {
                setting = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
                tab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            }
            else
            {
                var previousInfo = Tabs[index - 1];
                setting = UnityEngine.Object.Instantiate(gameSettings, customSettings[previousInfo.Key].transform.parent);
                tab = UnityEngine.Object.Instantiate(roleTab, customTabs[previousInfo.Key].transform);
            }

            customMenus[tabInfo.Key] = GetGameOptionsMenu(setting, tabInfo.Key);
            customSettings[tabInfo.Key] = setting;
            customTabHighlights[tabInfo.Key] = GetTabHighlight(tab, $"{tabInfo.Key}Tab", tabInfo.IconSprite);
            customTabs[tabInfo.Key] = tab;
        }
        
        gameTab.transform.position += Vector3.left * 3f;
        roleTab.transform.position += Vector3.left * 3f;

        for (var index = 0; index < Tabs.Count; index++)
        {
            var tabInfo = Tabs[index];
            var tab = customTabs[tabInfo.Key];
            if (index == 0)
            {
                tab.transform.position += Vector3.left * 2f;
            }
            else
            {
                tab.transform.localPosition += Vector3.right * 1f;
            }
        }

        var tabs = new List<GameObject> { gameTab, roleTab };
        tabs.AddRange(customTabs.Select(x => x.Value));
        
        var settingsHighlightMap = new Dictionary<GameObject, SpriteRenderer>
        {
            [gameSettingMenu.RegularGameSettings] = gameSettingMenu.GameSettingsHightlight,
            [gameSettingMenu.RolesSettings.gameObject] = gameSettingMenu.RolesSettingsHightlight,
        };
        
        foreach (var cs in customSettings)
        {
            settingsHighlightMap[cs.Value.gameObject] = customTabHighlights[cs.Key];
        }
        
        for (var i = 0; i < tabs.Count; i++)
        {
            var button = tabs[i].GetComponentInChildren<PassiveButton>();
            if (button == null) continue;
            var copiedIndex = i;
            button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            button.OnClick.AddListener((Action)(() => { SetListener(settingsHighlightMap, copiedIndex); }));
        }
        
        DestroyOptions(customMenus.Select(x => x.Value.GetComponentsInChildren<OptionBehaviour>()));

        var customOptions = new Dictionary<string, List<OptionBehaviour>>();
        var menus = new Dictionary<string, Transform>();
        var optionBehaviours = new Dictionary<string, List<OptionBehaviour>>();

        foreach (var tab in Tabs)
        {
            optionBehaviours[tab.Key] = customOptions[tab.Key] = new List<OptionBehaviour>();
            menus[tab.Key] = customMenus[tab.Key].transform;

            var options = CustomOptionManager.Options.Where(x => x.Tab == tab);

            foreach (var customOption in options)
            {
                if (customOption.OptionBehaviour == null)
                {
                    if (customOption.Type == CustomOption.Types.Boolean)
                    {
                        var boolOption = UnityEngine.Object.Instantiate(boolTemplate, menus[tab.Key]);
                        optionBehaviours[tab.Key].Add(boolOption);
                        boolOption.OnValueChanged = new Action<OptionBehaviour>(_ => { });
                        boolOption.TitleText.text = customOption.Name;
                        boolOption.CheckMark.enabled = boolOption.oldValue = customOption.CurrentSelection > 0;
                        customOption.OptionBehaviour = boolOption;
                    }
                    else
                    {
                        var stringOption = UnityEngine.Object.Instantiate(stringTemplate, menus[tab.Key]);
                        optionBehaviours[tab.Key].Add(stringOption);
                        stringOption.OnValueChanged = new Action<OptionBehaviour>(_ => { });
                        stringOption.TitleText.text = customOption.Name;
                        stringOption.Value = stringOption.oldValue = customOption.CurrentSelection;
                        stringOption.ValueText.text = customOption.DisplayValue;
                        customOption.OptionBehaviour = stringOption;
                    }
                }
                customOption.OptionBehaviour.gameObject.SetActive(true);
            }
            
        }
        
        SetOptions(customMenus.Values.ToList(), optionBehaviours.Values.ToList(), customSettings.Values.ToList());
    }
    
    private static void SetOptions(
        IReadOnlyList<GameOptionsMenu> menus,
        IReadOnlyList<List<OptionBehaviour>> options,
        IReadOnlyList<GameObject> settings)
    {
        if (menus.Count != options.Count || options.Count != settings.Count)
        {
            Specimen.Instance.Log.LogError("CustomOptionsMenuPatches: List counts are not equal");
            return;
        }

        for (var i = 0; i < menus.Count; i++)
        {
            menus[i].Children = options[i].ToArray();
            settings[i].gameObject.SetActive(false);
        }
    }
    
    private static void DestroyOptions(IEnumerable<IEnumerable<OptionBehaviour>> optionBehavioursList)
    {
        foreach (var option in optionBehavioursList.SelectMany(optionBehaviours => optionBehaviours))
        {
            UnityEngine.Object.Destroy(option.gameObject);
        }
    }
    
    private static void SetListener(Dictionary<GameObject, SpriteRenderer> settingsHighlightMap, int index)
    {
        foreach (var entry in settingsHighlightMap)
        {
            entry.Key.SetActive(false);
            entry.Value.enabled = false;
        }

        settingsHighlightMap.ElementAt(index).Key.SetActive(true);
        settingsHighlightMap.ElementAt(index).Value.enabled = true;
    }
    
    private static SpriteRenderer GetTabHighlight(GameObject tab, string tabName, Sprite tabSprite)
    {
        var highlight = tab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
        tab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = tabSprite;
        tab.name = $"{tabName}_Icon";

        return highlight;
    }

    private static bool TryRefreshNames()
    {
        var tab = Tabs.Count > 0 ? Tabs[0] : null;
        if (tab == null) return false;
        var obj = GameObject.Find(tab.Key);
        if (obj == null) return false;
        obj.transform.FindChild("GameGroup")
            .FindChild("Text")
            .GetComponent<TextMeshPro>()
            .SetText(tab.Title);
        return true;
    }

    private static GameOptionsMenu GetGameOptionsMenu(GameObject setting, string name)
    {
        var menu = setting.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
        setting.name = name;

        return menu;
    }
    
    internal static void UpdateTaskLimits(GameOptionsMenu gameOptionsMenu)
    {
        var commonTasksOption = gameOptionsMenu.Children.FirstOrDefault(x => x.name == "NumCommonTasks")?.TryCast<NumberOption>();
        if (commonTasksOption != null)
        {
            commonTasksOption.ValidRange = new FloatRange(MinCommonTaskCount, MaxCommonTaskCount);
        }

        var shortTasksOption = gameOptionsMenu.Children.FirstOrDefault(x => x.name == "NumShortTasks")?.TryCast<NumberOption>();
        if (shortTasksOption != null)
        {
            shortTasksOption.ValidRange = new FloatRange(MinShortTaskCount, MaxShortTaskCount);
        }

        var longTasksOption = gameOptionsMenu.Children.FirstOrDefault(x => x.name == "NumLongTasks")?.TryCast<NumberOption>();
        if (longTasksOption != null)
        {
            longTasksOption.ValidRange = new FloatRange(MinLongTaskCount, MaxLongTaskCount);
        }
    }
}