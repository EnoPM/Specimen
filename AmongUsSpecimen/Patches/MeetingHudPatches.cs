using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(MeetingHud))]
internal static class MeetingHudPatches
{
    [HarmonyPatch(nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    private static void StartPostfix()
    {
        GameEventManager.TriggerMeetingStarted();
    }
    
    [HarmonyPatch(nameof(MeetingHud.VotingComplete))]
    [HarmonyPostfix]
    private static void VotingCompletePostfix(MeetingHud __instance, Il2CppStructArray<MeetingHud.VoterState> states, GameData.PlayerInfo exiled, bool tie)
    {
        GameEventManager.TriggerMeetingEnded(exiled.Object);
    }
}