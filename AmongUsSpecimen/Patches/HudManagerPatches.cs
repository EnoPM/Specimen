using AmongUsSpecimen.VersionCheck;
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

    [HarmonyPatch(nameof(HudManager.Start))]
    [HarmonyPostfix]
    private static void StartPostfix()
    {
        VersionHandshakeManager.Window?.SetActive(true);
    }

    [HarmonyPatch(nameof(HudManager.OnDestroy))]
    [HarmonyPostfix]
    private static void OnDestroyPostfix()
    {
        VersionHandshakeManager.Window?.SetActive(false);
    }
}