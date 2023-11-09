using System;
using System.Collections.Generic;
using System.Linq;
using AmongUsSpecimen.UI.Components;
using BepInEx.Logging;
using UnityEngine;
using UniverseLib;
using UniverseLib.Config;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI;

public static class UiManager
{
    private const float StartupDelay = 1f;

    internal static UIBase UIBase { get; set; }
    private static GameObject UiRoot => UIBase?.RootObject;
    internal static RectTransform UiRootRect { get; private set; }
    internal static Canvas UiCanvas { get; private set; }
    internal static readonly List<UiWindow> Windows = new();
    private static OverlayWindow _overlay;
    internal static UiBehaviour Behaviour;

    public static event Action Inizialized;
    
    public static bool IsReady { get; private set; }

    internal static bool ShouldBlockClickOnGameElements =>
        Windows.Any(p => p is { Enabled: true, DisableClickThroughWindow: true });

    internal static void Init()
    {
        Universe.Init(StartupDelay, InitUi, HandleUiLog, new UniverseLibConfig
        {
            Disable_EventSystem_Override = true,
            Force_Unlock_Mouse = true
        });
    }

    private static void InitUi()
    {
        UIBase = UniversalUI.RegisterUI<UIBase>(Specimen.Guid, UiUpdated);
        if (UiRoot == null) return;
        UiRootRect = UiRoot.GetComponent<RectTransform>();
        UiCanvas = UiRoot.GetComponent<Canvas>();
        IsReady = true;
        _overlay = RegisterWindow<OverlayWindow>();
        Behaviour = Specimen.Instance.AddComponent<UiBehaviour>();
        Inizialized?.Invoke();
    }

    private static void HandleUiLog(string message, LogType logType)
    {
        var logLevel = logType switch
        {
            LogType.Log => LogLevel.Debug,
            LogType.Warning => LogLevel.Warning,
            LogType.Error => LogLevel.Error,
            LogType.Exception => LogLevel.Fatal,
            _ => LogLevel.Message
        };
        Specimen.Instance.Log.Log(logLevel, $"[UI] {message}");
    }

    private static void UiUpdated()
    {
        
    }

    internal static void UpdateOverlayState()
    {
        var state = Windows.Any(window => window.Enabled && window.HasOverlay);
        _overlay?.SetActive(state);
        if (!state) return;
        foreach (var window in Windows.Where(window => window.Enabled && window != _overlay))
        {
            window.SetActive(true, false);
        }
    }

    internal static void EnsureWindowValidPositions()
    {
        foreach (var window in Windows)
        {
            window.EnsureValidPosition();
        }
    }

    public static TWindow RegisterWindow<TWindow>(params object[] parameters) where TWindow : UiWindow
    {
        if (!IsReady)
        {
            throw new Exception($"Cannot register UiWindow before UiManager is ready");
        }
        
        var types = parameters.Select(x => x.GetType()).ToArray();
        
        var constructor = typeof(TWindow).GetConstructor(types);
        if (constructor == null)
        {
            throw new Exception($"Unable to find constructor for type {typeof(TWindow).Name} with argument types {string.Join(", " , types.Select(x => x.Name).ToList())}");
        }
        return (TWindow)constructor.Invoke(parameters);
    }
}