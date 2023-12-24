using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using TMPro;
using UnityEngine;

namespace AmongUsSpecimen.ModOptions;

public static class ModOptionUtility
{
    public const float MinCommonTaskCount = 0f;
    public const float MaxCommonTaskCount = 4f;
    public const float MinShortTaskCount = 0f;
    public const float MaxShortTaskCount = 23f;
    public const float MinLongTaskCount = 0f;
    public const float MaxLongTaskCount = 15f;

    internal static List<ModOptionTab> Tabs => ModOptionManager.Tabs;
    internal static List<BaseModOption> Options => ModOptionManager.Options;

    internal static readonly ModOptionPreset OnlinePreset = new()
    {
        IsSharable = true,
        Name = "Online",
        Values = new Dictionary<int, int>()
    };

    private static ModOptionPreset _currentPreset { get; set; } = new()
    {
        IsSharable = true,
        Name = "Default",
        Values = new Dictionary<int, int>()
    };

    internal static ModOptionPreset RealCurrentPreset => _currentPreset;

    internal static ModOptionPreset CurrentPreset
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

    internal static void ShareHostPreset()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost) return;
        RpcSetCurrentPreset(CurrentPreset);
    }

    [Rpc]
    public static void RpcSetCurrentPreset(ModOptionPreset preset)
    {
        CurrentPreset = preset;
    }

    [Rpc]
    public static void RpcSetValue(int key, int value)
    {
        SetValue_Internal(key, value);
    }

    internal static void RegisterOption(BaseModOption option)
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
                option.CurrentSelection = value;
            }
            else
            {
                CurrentPreset.SetValue(option.Id, value);
            }
        }
    }
    
    private static int CompareOption(BaseModOption a, BaseModOption b)
    {
        return string.Compare(a.Name, b.Name, StringComparison.InvariantCulture);
    }
    
    internal static void SetValue_Internal(int k, int v)
    {
        CurrentPreset.SetValue(k, v, !PlayerConditions.AmHostOrNotInGame());
    }

    private static void SetValue(this ModOptionPreset preset, int key, int value, bool updateCustomOption = false)
    {
        preset.Values[key] = value;
        if (!updateCustomOption) return;
        var option = key >= 0 && key < Options.Count ? Options[key] : null;
        if (option == null)
        {
            Specimen.Instance.Log.LogWarning($"[{nameof(ModOptionUtility)}] Trying to set value of non-existing custom option");
            return;
        }
        if (preset.Name != CurrentPreset.Name) return;
        option.CurrentSelection = value;
    }

    private static bool TryGetOption(OptionBehaviour optionBehaviour, out BaseModOption customOption)
    {
        customOption = Options.Find(x => x.OptionBehaviour == optionBehaviour);
        if (customOption != null)
        {
            optionBehaviour.OnValueChanged = (Action<OptionBehaviour>)EmptyAction;
        }
        return customOption != null;
    }
    
    internal static bool IsCustomOption(OptionBehaviour optionBehaviour)
    {
        return TryGetOption(optionBehaviour, out _);
    }

    internal static bool CustomOptionEnable(StringOption stringOption)
    {
        if (!TryGetOption(stringOption, out var option)) return true;
        stringOption.TitleText.text = option.DisplayName;
        stringOption.Value = stringOption.oldValue = option.CurrentSelection;
        stringOption.ValueText.text = option.DisplayValue;
        return false;
    }

    internal static bool CustomOptionEnable(ToggleOption boolOption)
    {
        if (!TryGetOption(boolOption, out var option)) return true;
        boolOption.TitleText.text = option.DisplayName;
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

    internal static void CustomOptionMenuUpdate(GameOptionsMenu optionsMenu)
    {
        var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
        if (gameSettingMenu != null && (gameSettingMenu.RegularGameSettings.active ||
                                        gameSettingMenu.RolesSettings.gameObject.active)) return;
        var activeTab = ModOptionManager.Tabs.Find(x => x.SettingsGameObject && x.SettingsGameObject.active);
        activeTab?.BehaviourUpdate(optionsMenu);
    }

    private static void EmptyAction(OptionBehaviour _)
    {
    }

    internal static void DestroyCustomOptionTabs()
    {
        foreach (var tab in Tabs)
        {
            foreach (var option in tab.Options)
            {
                if (option.OptionBehaviour)
                {
                    UnityEngine.Object.Destroy(option.OptionBehaviour);
                }

                option.OptionBehaviour = null;
            }
            if (tab.TabPositionObject)
            {
                UnityEngine.Object.Destroy(tab.TabPositionObject);
            }

            if (tab.TabObject)
            {
                UnityEngine.Object.Destroy(tab.TabObject);
            }

            if (tab.SettingsGameObject)
            {
                UnityEngine.Object.Destroy(tab.SettingsGameObject);
            }

            if (tab.Highlight && tab.Highlight.gameObject)
            {
                UnityEngine.Object.Destroy(tab.Highlight.gameObject);
            }

            if (tab.TitleTMP && tab.TitleTMP.gameObject)
            {
                UnityEngine.Object.Destroy(tab.TitleTMP.gameObject);
            }

            tab.TabPositionObject = null;
            tab.TabObject = null;
            tab.SettingsGameObject = null;
            tab.Highlight = null;
            tab.TitleTMP = null;
        }

        var gameTab = GameObject.Find("GameTab");
        if (!gameTab) return;
        var button = gameTab.GetComponentInChildren<PassiveButton>();
        if (!button) return;
        button.OnClick.Invoke();
    }
    
    private class DefaultGameObjectPositions
    {
        internal Vector3 RoleTab;
        internal Vector3 LocalRoleTab;
        internal Vector3 GameTab;
        internal Vector3 LocalGameTab;
    }

    private static DefaultGameObjectPositions _positionsCache;

    private static Vector3 Clone(this Vector3 vector)
    {
        return new Vector3(vector.x, vector.y, vector.z);
    }
    
    internal static void CreateCustomOptionTabs()
    {
        if (TryRefreshNames()) return;
        var stringTemplate = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
        var boolTemplate = UnityEngine.Object.FindObjectsOfType<ToggleOption>().FirstOrDefault();
        if (stringTemplate == null || boolTemplate == null) return;

        var gameSettings = GameObject.Find("Game Settings");
        if (!gameSettings) return;
        var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
        if (gameSettingMenu == null) return;
        
        var customSettings = new Dictionary<string, GameObject>();
        var customMenus = new Dictionary<string, GameOptionsMenu>();
        var customTabs = new Dictionary<string, GameObject>();
        var customTabHighlights = new Dictionary<string, SpriteRenderer>();
        
        var roleTab = GameObject.Find("RoleTab");
        var gameTab = GameObject.Find("GameTab");
        if (_positionsCache == null)
        {
            _positionsCache = new DefaultGameObjectPositions
            {
                RoleTab = roleTab.transform.position.Clone(),
                LocalRoleTab = roleTab.transform.localPosition.Clone(),
                GameTab = gameTab.transform.position.Clone(),
                LocalGameTab = gameTab.transform.localPosition.Clone()
            };
        }
        else
        {
            roleTab.transform.position = _positionsCache.RoleTab.Clone();
            roleTab.transform.localPosition = _positionsCache.LocalRoleTab.Clone();
            gameTab.transform.position = _positionsCache.GameTab.Clone();
            gameTab.transform.localPosition = _positionsCache.LocalGameTab.Clone();
        }

        var tabsToCreate = Tabs.Where(x => x.IsActive).ToList();
        
        for (var index = 0; index < tabsToCreate.Count; index++)
        {
            var tabInfo = tabsToCreate[index];
            GameObject setting;
            GameObject tab;
            if (index == 0)
            {
                setting = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
                tab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            }
            else
            {
                var previousInfo = tabsToCreate[index - 1];
                setting = UnityEngine.Object.Instantiate(gameSettings, customSettings[previousInfo.Key].transform.parent);
                tab = UnityEngine.Object.Instantiate(roleTab, customTabs[previousInfo.Key].transform);
            }

            customMenus[tabInfo.Key] = GetGameOptionsMenu(setting, $"{tabInfo.Key}OptionsMenu");
            tabInfo.TitleTMP = GetTitleTMP(setting);
            tabInfo.TitleTMP.SetText(tabInfo.Title);
            
            tabInfo.SettingsGameObject = customSettings[tabInfo.Key] = setting;
            tabInfo.Highlight = customTabHighlights[tabInfo.Key] = GetTabHighlight(tab, $"{tabInfo.Key}Tab", tabInfo.IconSprite);
            tabInfo.TabObject = tabInfo.Highlight.gameObject.transform.parent.gameObject;
            customTabs[tabInfo.Key] = tab;
        }
        
        gameTab.transform.position += Vector3.left * 3f;
        roleTab.transform.position += Vector3.left * 3f;

        for (var index = 0; index < tabsToCreate.Count; index++)
        {
            var tabInfo = tabsToCreate[index];
            var tab = customTabs[tabInfo.Key];
            if (index == 0)
            {
                tab.transform.position += Vector3.left * 2f;
            }
            else
            {
                tab.transform.localPosition += Vector3.right * 1f;
            }

            tabInfo.TabPositionObject = tab;
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
        
        var menus = new Dictionary<string, Transform>();
        var optionBehaviours = new Dictionary<string, List<OptionBehaviour>>();

        foreach (var tab in tabsToCreate)
        {
            optionBehaviours[tab.Key] = new();
            menus[tab.Key] = customMenus[tab.Key].transform;

            var options = ModOptionUtility.Options.Where(x => x.Tab == tab);

            foreach (var customOption in options)
            {
                if (customOption.OptionBehaviour == null)
                {
                    if (customOption.Type == OptionType.Boolean)
                    {
                        var boolOption = UnityEngine.Object.Instantiate(boolTemplate, menus[tab.Key]);
                        optionBehaviours[tab.Key].Add(boolOption);
                        boolOption.OnValueChanged = new Action<OptionBehaviour>(_ => { });
                        boolOption.TitleText.text = customOption.DisplayName;
                        boolOption.CheckMark.enabled = boolOption.oldValue = customOption.CurrentSelection > 0;
                        customOption.OptionBehaviour = boolOption;
                    }
                    else
                    {
                        var stringOption = UnityEngine.Object.Instantiate(stringTemplate, menus[tab.Key]);
                        optionBehaviours[tab.Key].Add(stringOption);
                        stringOption.OnValueChanged = new Action<OptionBehaviour>(_ => { });
                        stringOption.TitleText.text = customOption.DisplayName;
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
            if (entry.Key)
            {
                entry.Key.SetActive(false);
            }

            if (entry.Value)
            {
                entry.Value.enabled = false;
            }
        }
        
        var element = settingsHighlightMap.ElementAt(index);
        if (element.Key)
        {
            element.Key.SetActive(true);
        }

        if (element.Value)
        {
            element.Value.enabled = true;
        }
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
        if (Tabs.Count == 0) return false;
        var updated = false;
        foreach (var tab in Tabs.Where(x => x.IsActive))
        {
            if (!tab.TitleTMP) continue;
            tab.TitleTMP.SetText(tab.Title);
            updated = true;
        }
        
        return updated;
    }

    private static TextMeshPro GetTitleTMP(GameObject gameOptionMenu)
    {
        return gameOptionMenu.transform.Find("GameGroup")
            .FindChild("Text")
            .GetComponent<TextMeshPro>();
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