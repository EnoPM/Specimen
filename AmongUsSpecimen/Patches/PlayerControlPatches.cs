using HarmonyLib;
using Hazel;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(PlayerControl))]
internal static class PlayerControlPatches
{
    [HarmonyPatch(nameof(PlayerControl.HandleRpc))]
    [HarmonyPrefix]
    private static bool HandleRpcPrefix(PlayerControl __instance, byte callId, MessageReader reader)
    {
        if (callId != RpcManager.ReservedRpcCallId) return true;
        RpcManager.HandleRpc(__instance, reader);
        return false;
    }
}