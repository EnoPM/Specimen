using AmongUsSpecimen.VersionCheck;
using HarmonyLib;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(GameStartManager))]
internal static class GameStartManagerPatches
{
    [HarmonyPatch(nameof(GameStartManager.Update))]
    [HarmonyPrefix]
    private static void UpdatePrefix(GameStartManager __instance)
    {
        if (!GameData.Instance) return;
#if DEBUG
        __instance.MinPlayers = 1;
#endif
        if (__instance.startState == GameStartManager.StartingStates.Countdown)
        {
            if (PlayerConditions.AmHost())
            {
                __instance.startLabelText.text = "Stop";
                var pos = __instance.GameStartText.transform.localPosition;
                pos.y = 0.6f;
                __instance.GameStartText.transform.localPosition = pos;
                __instance.StartButton.gameObject.SetActive(true);
            }
        } else if (__instance.startState == GameStartManager.StartingStates.NotStarting)
        {
            __instance.startLabelText.text =
                DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.StartLabel);
            var shouldBlockGameStart = VersionHandshakeManager.ShouldBlockGameStart;
            __instance.StartButton.color = shouldBlockGameStart
                ? Palette.DisabledClear
                : Palette.EnabledColor;
            __instance.startLabelText.color = shouldBlockGameStart
                ? Palette.DisabledClear
                : Palette.EnabledColor;
        }
    }

    [Rpc]
    internal static void RpcResetStartState()
    {
        if (!DestroyableSingleton<GameStartManager>.InstanceExists) return;
        DestroyableSingleton<GameStartManager>.Instance.ResetStartState();
    }

    [HarmonyPatch(nameof(GameStartManager.BeginGame))]
    [HarmonyPrefix]
    private static bool BeginGamePrefix(GameStartManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        if (__instance.startState != GameStartManager.StartingStates.NotStarting)
        {
            RpcResetStartState();
            return false;
        }

        return !VersionHandshakeManager.ShouldBlockGameStart;
    }
}