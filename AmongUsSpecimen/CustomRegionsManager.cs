using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AmongUsSpecimen.Utils;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace AmongUsSpecimen;

public static class CustomRegionsManager
{
    public static readonly string RegionFileJson = Path.Combine(Paths.PluginPath, "Specimen", "Regions.json");
    
    private static ManualLogSource LogSource => Specimen.Instance.Log;
    
    private static readonly Dictionary<string, RegisteredRpc> AllRpc = new();
    private static void LogMessage(string message) => LogSource.LogMessage($"[{nameof(RpcManager)}] {message}");
    private static void LogDebug(string message) => LogSource.LogDebug($"[{nameof(RpcManager)}] {message}");
    private static void LogWarning(string message) => LogSource.LogWarning($"[{nameof(RpcManager)}] {message}");
    private static void LogError(string message) => LogSource.LogError($"[{nameof(RpcManager)}] {message}");

    public static IRegionInfo[] DefaultRegions { get; private set; } = new[]
    {
        CreateRegion("North America", "matchmaker.among.us", "https://matchmaker.among.us", 443, StringNames.ServerNA),
        CreateRegion("Europe", "matchmaker-eu.among.us", "https://matchmaker-eu.among.us", 443, StringNames.ServerEU),
        CreateRegion("Asia", "matchmaker-as.among.us", "https://matchmaker-as.among.us", 443, StringNames.ServerAS),
    };

    internal static IRegionInfo CreateRegion(string name, string pingServer, string host, ushort port = 443,
        StringNames translateName = StringNames.NoTranslation)
    {
        return new StaticHttpRegionInfo(name, translateName,
            pingServer,
            new Il2CppReferenceArray<ServerInfo>(new ServerInfo[1]
            {
                new("Http-1", host, port, false)
            })).Cast<IRegionInfo>();
    }

    internal static void RegisterAssembly(Assembly assembly)
    {
        var results = assembly.GetClassesByAttribute<CustomRegionAttribute>();
        LogMessage($"{results.Count} custom regions to register in assembly {assembly.FullName}");
        foreach (var result in results)
        {
            var r = result.Attribute;
            var name = r.Color != null ? ColorHelpers.Colorize(r.Color.Value, r.Name) : r.Name;
            DefaultRegions = DefaultRegions.AddToArray(CreateRegion(name, r.PingServer, r.Host, r.Port));
        }
    }
    
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CustomRegionAttribute : Attribute
{
    public readonly string Name;
    public readonly string PingServer;
    public readonly string Host;
    public readonly ushort Port;
    public readonly Color? Color;
    
    public CustomRegionAttribute(string name, string pingServer, string host, ushort port = 443, string color = "")
    {
        Name = name;
        PingServer = pingServer;
        Host = host;
        Port = port;
        Color = color == string.Empty ? null : ColorHelpers.FromHex(color);
    }
}