using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AmongUsSpecimen.Extensions;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Innersloth.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(ServerManager))]
internal static class ServerManagerPatches
{
    [HarmonyPatch(nameof(ServerManager.LoadServers))]
    [HarmonyPrefix]
    private static bool LoadServersPrefix(ServerManager __instance)
    {
        if (FileIO.Exists(CustomRegionsManager.RegionFileJson))
        {
            try
            {
                var jsonServerData = JsonConvert.DeserializeObject<ServerManager.JsonServerData>(
                    FileIO.ReadAllText(CustomRegionsManager.RegionFileJson), new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                var regions = CustomRegionsManager.CleanAndMerge(jsonServerData.Regions);
                __instance.AvailableRegions = regions;
                Specimen.Instance.Log.LogMessage($"Regions length: {regions.Length}");
                __instance.CurrentRegion = __instance.AvailableRegions[jsonServerData.CurrentRegionIdx.Wrap(regions.Length)];
                __instance.CurrentUdpServer = __instance.CurrentRegion.Servers.ToList().GetOneRandom();
                __instance.state = UpdateState.Success;
                __instance.SaveServers();
            }
            catch (Exception ex)
            {
                Specimen.Instance.Log.LogWarning(ex);
                __instance.StartCoroutine(ReselectRegionFromDefaults());
            }
        }
        else
        {
            __instance.StartCoroutine(ReselectRegionFromDefaults());
        }

        return false;
    }

    [HarmonyPatch(nameof(ServerManager.SaveServers))]
    [HarmonyPrefix]
    private static bool SaveServersPrefix(ServerManager __instance)
    {
        try
        {
            FileIO.WriteAllText(CustomRegionsManager.RegionFileJson, JsonConvert.SerializeObject(
                new ServerManager.JsonServerData
                {
                    CurrentRegionIdx = __instance.AvailableRegions.ToList()
                        .FindIndex(r => r.Name == __instance.CurrentRegion.Name),
                    Regions = __instance.AvailableRegions
                }, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }));
        }
        catch (Exception exception)
        {
            Specimen.Instance.Log.LogWarning($"{nameof(SaveServersPrefix)} error: {exception}");
        }

        return false;
    }

    private static IEnumerator ReselectRegionFromDefaults()
    {
        if (!ServerManager.InstanceExists) yield break;
        var sm = ServerManager.Instance;
        sm.AvailableRegions = CustomRegionsManager.DefaultRegions;
        var dnsLookup = CustomRegionsManager.DefaultRegions.Select(r => Dns.GetHostAddressesAsync(r.PingServer))
            .ToList();
        while (dnsLookup.Any(task => !task.IsCompleted))
        {
            yield return null;
        }

        var pings = new List<ServerManager.PingWrapper>();
        for (var index = 0; index < CustomRegionsManager.DefaultRegions.Length; index++)
        {
            var defaultRegion = CustomRegionsManager.DefaultRegions[index];
            IPAddress[] result;
            try
            {
                result = dnsLookup[index].ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Specimen.Instance.Log.LogWarning(ex);
                continue;
            }

            if (result == null || result.Length == 0)
            {
                Specimen.Instance.Log.LogWarning("DNS - no IPs resolved for " + defaultRegion.PingServer);
            }
            else
            {
                pings.Add(new ServerManager.PingWrapper(defaultRegion,
                    new Ping(result.ToList().GetOneRandom().ToString())));
            }
        }

        for (var timeElapsedSeconds = 0f;
             pings.Count > 0 && timeElapsedSeconds < 5f && !pings.Any(p => p.Ping.isDone && p.Ping.time >= 0);
             timeElapsedSeconds += Time.deltaTime)
        {
            yield return null;
        }

        var regionInfo = CustomRegionsManager.DefaultRegions.First();
        var num = int.MaxValue;
        foreach (var pingWrapper in pings)
        {
            if (pingWrapper.Ping.isDone && pingWrapper.Ping.time >= 0)
            {
                if (pingWrapper.Ping.time < num)
                {
                    regionInfo = pingWrapper.Region;
                    num = pingWrapper.Ping.time;
                }
            }

            pingWrapper.Ping.DestroyPing();
        }

        sm.CurrentRegion = regionInfo.Duplicate();
        sm.ReselectServer();
        sm.SaveServers();
    }
}