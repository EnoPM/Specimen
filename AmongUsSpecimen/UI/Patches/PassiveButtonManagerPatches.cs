using HarmonyLib;

namespace AmongUsSpecimen.UI.Patches;

[HarmonyPatch(typeof(PassiveButtonManager))]
public static class PassiveButtonManagerPatches
{
    public static bool IsUiOpen { get; private set; }

    public static void UpdateState()
    {
        IsUiOpen = UiManager.ShouldBlockClickOnGameElements;
    }
    
    [HarmonyPatch(nameof(PassiveButtonManager.Update))]
    [HarmonyPrefix]
    private static bool PassiveButtonManagerUpdatePrefix()
    {
        return !IsUiOpen;
    }
}