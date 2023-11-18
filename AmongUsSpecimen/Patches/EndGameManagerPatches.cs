using HarmonyLib;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(EndGameManager))]
internal static class EndGameManagerPatches
{
    [HarmonyPatch(nameof(EndGameManager.SetEverythingUp))]
    [HarmonyPostfix]
    private static void SetEverythingUpPostfix()
    {
        GameEventManager.TriggerGameEnded();
    }
}