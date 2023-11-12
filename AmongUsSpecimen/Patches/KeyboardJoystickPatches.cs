using HarmonyLib;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(KeyboardJoystick))]
internal static class KeyboardJoystickPatches
{
    private static int PrimaryKeyBind => RewiredConsts.Action.ActionPrimary;
    private static int SecondaryKeyBind => RewiredConsts.Action.ActionSecondary;
    private static int TertiaryKeyBind => RewiredConsts.Action.ActionTertiary;
    private static int QuaternaryKeyBind => RewiredConsts.Action.ActionQuaternary;
    private static int ToggleMapKeyBind => RewiredConsts.Action.ToggleMap;
    private static int UseVentKeyBind => RewiredConsts.Action.UseVent;
    
    [HarmonyPatch(nameof(KeyboardJoystick.HandleHud))]
    [HarmonyPrefix]
    private static bool HandleHudPrefix(KeyboardJoystick __instance)
    {
        if (!DestroyableSingleton<HudManager>.InstanceExists) return false;
        var hudManager = DestroyableSingleton<HudManager>.Instance;
        var me = KeyboardJoystick.player;
        var player = PlayerControl.LocalPlayer;
        if (me.GetButtonDown(TertiaryKeyBind))
            hudManager.ReportButton.DoClick();
        if (me.GetButtonDown(PrimaryKeyBind))
            hudManager.UseButton.DoClick();
        if (me.GetButtonDown(ToggleMapKeyBind) && !hudManager.Chat.IsOpenOrOpening)
            hudManager.ToggleMapVisible(GameManager.Instance.GetMapOptions());
        if (me.GetButtonDown(QuaternaryKeyBind))
            hudManager.AbilityButton.DoClick();
        if (player.Data == null || player.Data.Role == null) return false;
        if (PlayerConditions.CanUseKillButtonKeyBind(player) && me.GetButtonDown(SecondaryKeyBind))
            hudManager.KillButton.DoClick();
        if (!PlayerConditions.CanUseVentButtonKeyBind(player) || !me.GetButtonDown(UseVentKeyBind)) return false;
        DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.DoClick();
        return false;
    }
}