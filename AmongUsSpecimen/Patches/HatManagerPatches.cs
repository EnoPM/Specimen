using System;
using System.Collections.Generic;
using System.Linq;
using AmongUsSpecimen.Cosmetics;
using Cpp2IL.Core.Extensions;
using HarmonyLib;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(HatManager))]
internal static class HatManagerPatches
{
    private static bool hatIsRunning;
    private static bool hatIsLoaded;
    private static List<HatData> allHats;
        
    [HarmonyPatch(nameof(HatManager.GetHatById))]
    [HarmonyPrefix]
    private static void GetHatByIdPrefix(HatManager __instance)
    {
        if (hatIsRunning || hatIsLoaded) return;
        hatIsRunning = true;
        // Maybe we can use lock keyword to ensure simultaneous list manipulations ?
        allHats = __instance.allHats.ToList();
        var cache = CustomCosmeticsManager.UnregisteredHats.Clone();
        foreach (var hat in cache)
        {
            try
            {
                allHats.Add(CustomCosmeticsManager.CreateHatBehaviour(hat));
                CustomCosmeticsManager.UnregisteredHats.Remove(hat);
                CustomCosmeticsManager.RegisteredHats[hat.Name] = hat;
            }
            catch (Exception err)
            {
                Specimen.Instance.Log.LogWarning($"GetHatByIdPrefix: error for hat {hat.Name}");
            }
        }
        cache.Clear();

        __instance.allHats = allHats.ToArray();
        hatIsLoaded = true;
    }
        
    [HarmonyPatch(nameof(HatManager.GetHatById))]
    [HarmonyPostfix]
    private static void GetHatByIdPostfix()
    {
        hatIsRunning = false;
    }
}