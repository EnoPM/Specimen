using AmongUsSpecimen.ModOptions;
using HarmonyLib;
using static AmongUsSpecimen.ModOptions.ModOptionUtility;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(KeyValueOption))]
internal static class KeyValueOptionPatches
{
    [HarmonyPatch(nameof(KeyValueOption.Increase))]
    [HarmonyPatch(nameof(KeyValueOption.Decrease))]
    [HarmonyPostfix]
    private static void UpdateValuePostfix(KeyValueOption __instance)
    {
        if (IsCustomOption(__instance)) return;
        OptionStorage.SaveVanillaOptions();
    }
}

[HarmonyPatch(typeof(NumberOption))]
internal static class NumberOptionPatches
{
    [HarmonyPatch(nameof(NumberOption.Increase))]
    [HarmonyPatch(nameof(NumberOption.Decrease))]
    [HarmonyPostfix]
    private static void UpdateValuePostfix(NumberOption __instance)
    {
        if (IsCustomOption(__instance)) return;
        OptionStorage.SaveVanillaOptions();
    }
}

[HarmonyPatch(typeof(PlayerOption))]
internal static class PlayerOptionPatches
{
    [HarmonyPatch(nameof(PlayerOption.Increase))]
    [HarmonyPatch(nameof(PlayerOption.Decrease))]
    [HarmonyPostfix]
    private static void UpdateValuePostfix(PlayerOption __instance)
    {
        if (IsCustomOption(__instance)) return;
        OptionStorage.SaveVanillaOptions();
    }
}

[HarmonyPatch(typeof(RoleOptionSetting))]
internal static class RoleOptionSettingPatches
{
    [HarmonyPatch(nameof(RoleOptionSetting.DecreaseChance))]
    [HarmonyPatch(nameof(RoleOptionSetting.IncreaseChance))]
    [HarmonyPatch(nameof(RoleOptionSetting.DecreaseCount))]
    [HarmonyPatch(nameof(RoleOptionSetting.IncreaseCount))]
    [HarmonyPostfix]
    private static void UpdateValuePostfix(RoleOptionSetting __instance)
    {
        if (IsCustomOption(__instance)) return;
        OptionStorage.SaveVanillaOptions();
    }
}