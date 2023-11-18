using HarmonyLib;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(HudManager))]
internal static class HudManagerPatches
{
    [HarmonyPatch(nameof(HudManager.OnGameStart))]
    [HarmonyPostfix]
    private static void OnGameStartPostfix(HudManager __instance)
    {
        GameEventManager.TriggerGameStarted();
    }

    [HarmonyPatch(nameof(HudManager.Update))]
    [HarmonyPostfix]
    private static void UpdatePostfix(HudManager __instance)
    {
        __instance.GameSettings.gameObject.SetActive(false);
    }
}