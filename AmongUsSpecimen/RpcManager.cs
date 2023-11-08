using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using AmongUsSpecimen.Utils;
using AmongUsSpecimen.Utils.Converters;
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

    private static readonly JsonSerializerOptions SerializerOptions = new() { Converters = { new UnityColorConverter(), new PlayerControlConverter() }};

    internal static void Load()
    {
        foreach (var assembly in AssemblyHelpers.AllAssemblies)
        {
            LoadAssembly(assembly);
        }
    }

    public static void LoadAssembly(Assembly assembly)
    {
        var methods = assembly.GetMethodsByAttribute<RpcAttribute>();
        LogMessage($"{methods.Count} methods to register in assembly {assembly.FullName}");
        foreach (var method in methods)
        {
            RegisterMethodResult(method);
        }
    }

    private static void RegisterMethodResult(AttributeHelpers.AttributeMethodResult<RpcAttribute> methodResult)
    {
        var id = $"{methodResult.Method.DeclaringType?.FullName}#{methodResult.Method.Name}";
        if (AllRpc.TryGetValue(id, out var alreadyRegistered))
        {
            LogWarning(
                $"Rpc {id} already registered for method {alreadyRegistered.OriginalMethod.Name} in class {alreadyRegistered.OriginalMethod.DeclaringType?.FullName}");
            return;
        }

        var original = Harmony.Patch(methodResult.Method);
        var prefix = new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.DynamicPrefix)));
        var declaringType = methodResult.Method.DeclaringType;
        LogDebug($"Trying to register rpc method of type {declaringType?.FullName}");
        var invoker = new MethodInvoker(original, methodResult.Method.IsStatic ? null : Singletons.Get(methodResult.Declaring));
        Harmony.Patch(methodResult.Method, prefix);
        AllRpc[id] = new RegisteredRpc(methodResult.Method, methodResult.Attribute, invoker);
        LogDebug($"Registered rpc for method {id}");
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
            PlayerControl sender = null;

            for (var i = 0; i < __args.Length; i++)
            {
                if (localParameterIndexes.Contains(i))
                {
                    sender = (PlayerControl)__args[i];
                    continue;
                }
                data.Add(JsonSerializer.Serialize(__args[i]));
            }

            var id = $"{__originalMethod.DeclaringType?.FullName}#{__originalMethod.Name}";

            new RpcData { Id = id, Data = data }.Send(sender);

            return rpc.Attribute.Execution == LocalExecution.Before && sender && sender != PlayerControl.LocalPlayer;
        }
    }

    internal static void HandleRpc(PlayerControl sender, MessageReader reader)
    {
        var data = RpcData.Read(reader);
        if (!AllRpc.TryGetValue(data.Id, out var rpc))
        {
            LogWarning($"HandleRpc: Unknown rpc: {data.Id}");
            return;
        }

        if (sender.AmOwner)
        {
            LogWarning($"HandleRpc: AmOwner of rpc call {data.Id}");
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
                try
                {
                    var deserializerMethod = typeof(JsonSerializer).GetMethods().FirstOrDefault(x => x.Name == nameof(JsonSerializer.Deserialize) && x.ContainsGenericParameters && x.GetParameters().Length == 2 && x.GetParameters()[0].ParameterType == typeof(string) && x.GetParameters()[1].ParameterType == typeof(JsonSerializerOptions));
                    if (deserializerMethod == null)
                    {
                        LogError($"deserializerMethod not found");
                        return;
                    }
                    var deserialized = deserializerMethod.MakeGenericMethod(p.ParameterType)
                        .Invoke(null, new object[] { data.Data[index], SerializerOptions });
                    if (deserialized == null)
                    {
                        LogError($"HandleRpc: Unable to deserialize data from rpc {data.Id}");
                        args.Add(null);
                        return;
                    }
                    args.Add(deserialized);
                }
                catch (Exception exception)
                {
                    LogError($"Unable to deserialize rpc data: {exception}");
                    return;
                }
            }
        }

        var declaringType = rpc.OriginalMethod.DeclaringType;
        if (declaringType == null)
        {
            LogError($"HandleRpc: Unable to execute rpc {data.Id}: no declaring type!");
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

    internal void Send(PlayerControl sender = null)
    {
        sender = sender ? sender : PlayerControl.LocalPlayer;
        var writer = AmongUsClient.Instance.StartRpcImmediately(sender.NetId,
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