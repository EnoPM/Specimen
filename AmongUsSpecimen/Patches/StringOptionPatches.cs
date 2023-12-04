using HarmonyLib;
using static AmongUsSpecimen.ModOptions.ModOptionUtility;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(StringOption))]
internal static class StringOptionPatches
{
    [HarmonyPatch(nameof(StringOption.OnEnable))]
    [HarmonyPrefix]
    private static bool OnEnablePrefix(StringOption __instance)
    {
        return CustomOptionEnable(__instance);
    }
    
    [HarmonyPatch(nameof(StringOption.Increase))]
    [HarmonyPrefix]
    private static bool IncreasePrefix(StringOption __instance)
    {
        return CustomOptionIncrease(__instance);
    }
    
    [HarmonyPatch(nameof(StringOption.Decrease))]
    [HarmonyPrefix]
    private static bool DecreasePrefix(StringOption __instance)
    {
        return CustomOptionDecrease(__instance);
    }
}