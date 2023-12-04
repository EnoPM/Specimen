using HarmonyLib;
using static AmongUsSpecimen.ModOptions.ModOptionUtility;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(GameOptionsMenu))]
internal static class GameOptionsMenuPatches
{
    [HarmonyPatch(nameof(GameOptionsMenu.Start))]
    [HarmonyPostfix]
    private static void StartPostfix(GameOptionsMenu __instance)
    {
        UpdateTaskLimits(__instance);
        CreateCustomOptionTabs();
    }

    [HarmonyPatch(nameof(GameOptionsMenu.Update))]
    [HarmonyPostfix]
    private static void UpdatePostfix(GameOptionsMenu __instance)
    {
        CustomOptionMenuUpdate(__instance);
    }
}