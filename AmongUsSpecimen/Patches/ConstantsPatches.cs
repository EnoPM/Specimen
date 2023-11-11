using HarmonyLib;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(Constants))]
internal static class ConstantsPatches
{
    [HarmonyPatch(nameof(Constants.GetBroadcastVersion))]
    [HarmonyPostfix]
    private static void GetBroadcastVersionPostfix(ref int __result)
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame)
        {
            __result += 25;
        }
    }
}