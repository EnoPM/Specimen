using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AmongUsSpecimen.Utils;
using BepInEx.Logging;
using HarmonyLib;
using Hazel;

namespace AmongUsSpecimen;

public static class RpcManager
{
    internal const byte ReservedRpcCallId = 249;
    
    private static ManualLogSource LogSource => Specimen.Instance.Log;
    private static Harmony Harmony => Specimen.Harmony;
    
    private static readonly Dictionary<string, RegisteredRpc> AllRpc = new();
    private static void LogMessage(string message) => LogSource.LogMessage($"[{nameof(RpcManager)}] {message}");
    private static void LogDebug(string message) => LogSource.LogDebug($"[{nameof(RpcManager)}] {message}");
    private static void LogWarning(string message) => LogSource.LogWarning($"[{nameof(RpcManager)}] {message}");
    private static void LogError(string message) => LogSource.LogError($"[{nameof(RpcManager)}] {message}");

    internal static void Load()
    {
        var methods = AttributeHelpers.GetMethodsByAttribute<RpcAttribute>(AssemblyHelpers.AllAssemblies);
        LogMessage($"{methods.Count} methods to register");
        foreach (var method in methods)
        {
            var id = $"{method.Method.DeclaringType?.FullName}#{method.Method.Name}";
            if (AllRpc.TryGetValue(id, out var alreadyRegistered))
            {
                LogWarning(
                    $"Rpc {id} already registered for method {alreadyRegistered.OriginalMethod.Name} in class {alreadyRegistered.OriginalMethod.DeclaringType?.FullName}");
                continue;
            }

            var original = Harmony.Patch(method.Method);
            var prefix = new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.DynamicPrefix)));
            var declaringType = method.Method.DeclaringType;
            LogDebug($"Trying to register rpc method of type {declaringType?.FullName}");
            var invoker = new MethodInvoker(original, method.Method.IsStatic ? null : Singletons.Get(method.Declaring));
            Harmony.Patch(method.Method, prefix);
            AllRpc[id] = new RegisteredRpc(method.Method, method.Attribute, invoker);
            LogDebug($"Registered rpc for method {id}");
        }
    }

    private static class Patches
    {
        public static bool DynamicPrefix(MethodBase __originalMethod, object[] __args)
        {
            var rpc = AllRpc.Values.FirstOrDefault(r => r.OriginalMethod == __originalMethod);
            if (rpc == null) return false;

            var data = new List<string>();

            var parameters = rpc.OriginalMethod.GetParameters().ToList();
            var localParameterIndexes = new List<int> { parameters.FindIndex(IsSenderParameter) };

            for (var i = 0; i < __args.Length; i++)
            {
                if (localParameterIndexes.Contains(i)) continue;
                data.Add(JsonSerializer.Serialize(__args[i]));
            }

            var id = $"{__originalMethod.DeclaringType?.FullName}#{__originalMethod.Name}";

            new RpcData { Id = id, Data = data }.Send();

            return rpc.Attribute.Execution != LocalExecution.None;
        }
    }

    internal static void HandleRpc(PlayerControl sender, MessageReader reader)
    {
        var data = RpcData.Read(reader);
        if (!AllRpc.TryGetValue(data.Id, out var rpc))
        {
            LogWarning($"Unknown rpc: {data.Id}");
            return;
        }

        if (sender.AmOwner)
        {
            LogWarning($"AmOwner of rpc call {data.Id}");
            if (rpc.Attribute.Execution != LocalExecution.After) return;
        }
        var parameters = rpc.OriginalMethod.GetParameters();
        var args = new List<object>();
        if (!rpc.OriginalMethod.IsStatic)
        {
            args.Add(rpc.Invocation.Invoker);
        }

        var index = -1;
        foreach (var p in parameters)
        {
            if (IsSenderParameter(p))
            {
                args.Add(sender);
            }
            else
            {
                index++;
                var deserializerMethod = typeof(JsonSerializer).GetMethod(nameof(JsonSerializer.Deserialize));
                var deserialized = deserializerMethod?.MakeGenericMethod(p.ParameterType)
                    .Invoke(null, new object[] { data.Data[index] });
                if (deserialized == null)
                {
                    LogError($"[RPC] Unable to deserialize data from rpc {data.Id}");
                    args.Add(null);
                    return;
                }
                args.Add(deserialized);
            }
        }

        var declaringType = rpc.OriginalMethod.DeclaringType;
        if (declaringType == null)
        {
            LogError($"Unable to execute rpc {data.Id}: no declaring type!");
            return;
        }

        rpc.Invocation.Method.Invoke(rpc.Invocation.Invoker, args.ToArray());
    }

    private static bool IsSenderParameter(ParameterInfo parameter)
    {
        if (parameter.Name != "__sender") return false;
        return parameter.ParameterType == typeof(PlayerControl);
    }
}

internal class RegisteredRpc
{
    public readonly MethodInfo OriginalMethod;
    public readonly RpcAttribute Attribute;
    public readonly MethodInvoker Invocation;

    public RegisteredRpc(MethodInfo originalMethod, RpcAttribute attribute, MethodInvoker invocation)
    {
        OriginalMethod = originalMethod;
        Attribute = attribute;
        Invocation = invocation;
    }
}

internal class MethodInvoker
{
    public readonly MethodInfo Method;
    public readonly object Invoker;

    public MethodInvoker(MethodInfo method, object invoker = null)
    {
        Method = method;
        Invoker = invoker;
    }
}

internal class RpcData
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("data")] public List<string> Data { get; set; }

    internal void Send()
    {
        var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
            RpcManager.ReservedRpcCallId, SendOption.Reliable);
        writer.Write(Serialize());
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    internal static RpcData Read(MessageReader reader)
    {
        return Deserialize(reader.ReadString());
    }

    internal string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }

    internal static RpcData Deserialize(string serialized)
    {
        return JsonSerializer.Deserialize<RpcData>(serialized);
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class RpcAttribute : Attribute
{
    public readonly LocalExecution Execution;

    public RpcAttribute(LocalExecution execution = LocalExecution.After)
    {
        Execution = execution;
    }
}

public enum LocalExecution
{
    None,
    Before,
    After
}