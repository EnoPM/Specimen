using HarmonyLib;
using InnerNet;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(AmongUsClient))]
internal static class AmongUsClientPatches
{
    [HarmonyPatch(nameof(AmongUsClient.OnPlayerJoined))]
    [HarmonyPostfix]
    private static void OnPlayerJoinedPostfix(AmongUsClient __instance, ClientData data)
    {
        GameEventManager.TriggerPlayerJoined(data.Character);
    }

    [HarmonyPatch(nameof(AmongUsClient.OnPlayerLeft))]
    [HarmonyPrefix]
    private static void OnPlayerLeftPrefix(AmongUsClient __instance, ClientData data, DisconnectReasons reason)
    {
        GameEventManager.TriggerPlayerLeft(data.Character);
    }
}