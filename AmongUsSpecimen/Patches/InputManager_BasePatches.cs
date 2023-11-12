using HarmonyLib;
using Rewired;
using Rewired.Data;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(InputManager_Base))]
public static class InputManager_BasePatches
{
    [HarmonyPatch(nameof(InputManager_Base.Awake))]
    [HarmonyPrefix]
    private static void AwakePrefix(InputManager_Base __instance)
    {
        foreach (var keyBind in CustomKeyBindManager.CustomKeyBinds)
        {
            __instance.userData.RegisterBind(keyBind.Key, keyBind.Value.Description, keyBind.Value.DefaultKeyCode);
        }
    }
    
    private static int RegisterBind(this UserData self, string name, string description, KeyboardKeyCode keycode, int elementIdentifierId = -1, int category = 0, InputActionType type = InputActionType.Button)
    {
        self.AddAction(category);
        var action = self.GetAction(self.actions.Count - 1)!;

        action.name = name;
        action.descriptiveName = description;
        action.categoryId = category;
        action.type = type;
        action.userAssignable = true;

        var a = new ActionElementMap();
        a._elementIdentifierId = elementIdentifierId;
        a._actionId = action.id;
        a._elementType = ControllerElementType.Button;
        a._axisContribution = Pole.Positive;
        a._keyboardKeyCode = keycode;
        a._modifierKey1 = ModifierKey.None;
        a._modifierKey2 = ModifierKey.None;
        a._modifierKey3 = ModifierKey.None;
        self.keyboardMaps._items[0].actionElementMaps.Add(a);
        self.joystickMaps._items[0].actionElementMaps.Add(a);
            
        return action.id;
    }
}