using AmongUsSpecimen.Extensions;
using HarmonyLib;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(HatsTab))]
internal static class HatsTabPatches
{
    
    [HarmonyPatch(nameof(HatsTab.OnEnable))]
    [HarmonyPostfix]
    private static void OnEnablePostfix(HatsTab __instance)
    {
        __instance.SetupCustomHats();
    }
}