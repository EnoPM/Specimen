using System;
using UnityEngine;
using UniverseLib.Input;

namespace AmongUsSpecimen.UI;
using IM = UniverseLib.Input.InputManager;

public static class InputManager
{
    public static InputType CurrentType => IM.CurrentType;

    public static Vector3 MousePosition => IM.MousePosition;
    
    public static Vector2 MouseScrollDelta => IM.MouseScrollDelta;

    public static bool GetKeyDown(KeyCode key) => IM.GetKeyDown(key);

    public static bool GetKey(KeyCode key) => IM.GetKey(key);

    public static bool GetKeyUp(KeyCode key) => IM.GetKeyUp(key);

    public static bool GetMouseButtonDown(int btn) => IM.GetMouseButtonDown(btn);
    
    public static bool GetMouseButton(int btn) => IM.GetMouseButton(btn);
    
    public static bool GetMouseButtonUp(int btn) => IM.GetMouseButtonUp(btn);

    public static void ResetInputAxes() => IM.ResetInputAxes();

    public static bool Rebinding
    {
        get => IM.Rebinding;
        set => IM.Rebinding = value;
    }

    public static KeyCode? LastRebindKey
    {
        get => IM.LastRebindKey;
        set => IM.LastRebindKey = value;
    }

    public static void BeginRebind(Action<KeyCode> onSelection, Action<KeyCode?> onFinished) =>
        IM.BeginRebind(onSelection, onFinished);

    public static void EndRebind() => IM.EndRebind();
}