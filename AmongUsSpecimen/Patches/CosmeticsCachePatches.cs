﻿using AmongUsSpecimen.Cosmetics;
using HarmonyLib;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(CosmeticsCache))]
internal static class CosmeticsCachePatches
{
    [HarmonyPatch(nameof(CosmeticsCache.GetHat))]
    [HarmonyPrefix]
    private static bool GetHatPrefix(string id, ref HatViewData __result)
    {
        Specimen.Instance.Log.LogMessage($"trying to load hat {id} from cosmetics cache");
        return !CustomCosmeticsManager.HatViewDataCache.TryGetValue(id, out __result);
    }
}