using HarmonyLib;
using static AmongUsSpecimen.ModOptions.ModOptionUtility;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(ToggleOption))]
internal static class ToggleOptionPatches
{
    [HarmonyPatch(nameof(ToggleOption.OnEnable))]
    [HarmonyPrefix]
    private static bool OnEnablePrefix(ToggleOption __instance)
    {
        return CustomOptionEnable(__instance);
    }
    
    [HarmonyPatch(nameof(ToggleOption.Toggle))]
    [HarmonyPrefix]
    private static bool TogglePrefix(ToggleOption __instance)
    {
        return CustomOptionIncrease(__instance);
    }
}