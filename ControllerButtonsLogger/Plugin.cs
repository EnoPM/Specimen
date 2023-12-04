using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Rewired;
using UnityEngine;

namespace ControllerButtonsLogger;

[BepInPlugin(Guid, Name, Version)]
public class Plugin : BasePlugin
{
    public const string Guid = "controller.buttons.logger.eno.pm";
    private const string Name = "ControllerButtonsLogger";
    private const string Version = "1.0.0";

    internal static ManualLogSource Logger { get; private set; }
    
    public override void Load()
    {
        // Plugin startup logic
        Logger = Log;
        Log.LogInfo($"Plugin {Name} is loaded!");
        AddComponent<ControllerButtonsLoggerBehaviour>();
    }
}

internal class ControllerButtonsLoggerBehaviour : MonoBehaviour
{
    private static int? _lastButtonPressed = null;
    
    private void Update()
    {
        var me = ConsoleJoystick.player;
        if (me == null) return;
        for (var i = 0; i < 100; i++)
        {
            if (!me.GetButtonDown(i)) continue;
            if (_lastButtonPressed == i)
            {
                Plugin.Logger.LogMessage($"Button pressed two times: {i}");
            }

            _lastButtonPressed = i;
        }
    }
}