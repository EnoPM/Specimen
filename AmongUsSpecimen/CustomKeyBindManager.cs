using System;
using System.Collections.Generic;
using System.Reflection;
using AmongUsSpecimen.Utils;
using Rewired;

namespace AmongUsSpecimen;

internal static class CustomKeyBindManager
{
    internal static readonly Dictionary<string, CustomKeyBind> CustomKeyBinds = new();
    
    internal static void RegisterAssembly(Assembly assembly)
    {
        var results = assembly.GetClassesByAttribute<CustomKeyBindAttribute>();
        foreach (var result in results)
        {
            var key = result.Attribute.ActionName;
            if (CustomKeyBinds.ContainsKey(key))
            {
                Specimen.Instance.Log.LogWarning($"Custom key bind {key} creation skipped in assembly {assembly.FullName}: a key bind with same name is already registered");
                continue;
            }

            CustomKeyBinds[key] = new CustomKeyBind(key, result.Attribute.Description, result.Attribute.DefaultKeyCode);
        }
    }
    
    internal class CustomKeyBind
    {
        public readonly string ActionName;
        public readonly string Description;
        public readonly KeyboardKeyCode DefaultKeyCode;
        
        public CustomKeyBind(string actionName, string description, KeyboardKeyCode defaultKeyCode)
        {
            ActionName = actionName;
            Description = description;
            DefaultKeyCode = defaultKeyCode;
        }
    }
}

public static class CustomKeyBinds
{
    private static Player Player => ReInput.players?.GetPlayer(0);

    public static bool GetKeyboardButtonDown(string actionName) => Player?.GetButtonDown(actionName) == true;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CustomKeyBindAttribute : Attribute
{
    public readonly string ActionName;
    public readonly string Description;
    public readonly KeyboardKeyCode DefaultKeyCode;

    public CustomKeyBindAttribute(string actionName, string description, KeyboardKeyCode defaultKeyCode)
    {
        ActionName = actionName;
        Description = description;
        DefaultKeyCode = defaultKeyCode;
    }
}