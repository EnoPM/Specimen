using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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
    private const string SenderParameterName = "__sender";
    private const string ReceiverParameterName = "__receiver";
    private static readonly Type SenderParameterType = typeof(PlayerControl);
    private static readonly Type ReceiverParameterType = typeof(PlayerControl);

    private static ManualLogSource LogSource => Specimen.Instance.Log;
    private static Harmony Harmony => Specimen.Harmony;

    private static readonly Dictionary<string, RegisteredRpc> AllRpc = new();
    private static void LogMessage(string message) => LogSource.LogMessage($"[{nameof(RpcManager)}] {message}");
    private static void LogDebug(string message) => LogSource.LogDebug($"[{nameof(RpcManager)}] {message}");
    private static void LogWarning(string message) => LogSource.LogWarning($"[{nameof(RpcManager)}] {message}");
    private static void LogError(string message) => LogSource.LogError($"[{nameof(RpcManager)}] {message}");

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters =
        {
            new UnityColorConverter(),
            new PlayerControlConverter(),
            new SystemGuidConverter(),
            new SystemVersionConverter()
        }
    };

    public static void RegisterAssembly(Assembly assembly)
    {
        var methods = assembly.GetMethodsByAttribute<RpcAttribute>();
        LogMessage($"{methods.Count} rpc methods to register in assembly {assembly.FullName}");
        foreach (var method in methods)
        {
            RegisterMethodResult(method);
        }
    }

    private static string GetSignature(MethodInfo methodInfo)
    {
        var sb = new StringBuilder();
        sb.Append(methodInfo.DeclaringType?.FullName)
            .Append(methodInfo.Name);
        foreach (var parameter in methodInfo.GetParameters())
        {
            sb.Append(parameter.ParameterType.FullName)
                .Append(parameter.Name)
                .Append(parameter.Position)
                .Append(parameter.IsOptional);
        }

        sb.Append(methodInfo.ReturnType.FullName);

        return sb.ToString();
    }

    private static string GetSignatureHash(MethodInfo methodInfo)
    {
        using var crypto = SHA1.Create();
        var signature = GetSignature(methodInfo);
        var source = Encoding.UTF8.GetBytes(signature);
        var hash = crypto.ComputeHash(source);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    private static void RegisterMethodResult(AttributeHelpers.AttributeMethodResult<RpcAttribute> methodResult)
    {
        var id = GetSignatureHash(methodResult.Method);
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
        var invoker = new MethodInvoker(original,
            methodResult.Method.IsStatic ? null : Singletons.Get(methodResult.Declaring));
        Harmony.Patch(methodResult.Method, prefix);
        AllRpc[id] = new RegisteredRpc(methodResult.Method, methodResult.Attribute, invoker);
        LogDebug($"Registered rpc for method {id}");
    }

    private static class Patches
    {
        public static bool DynamicPrefix(MethodBase __originalMethod, object[] __args)
        {
            var data = AllRpc.FirstOrDefault(r => r.Value.OriginalMethod == __originalMethod);
            if (data.Value == null) return false;
            var (id, rpc) = data;
            TriggerRpcCall(id, rpc, __args);
            return rpc.Attribute.Execution == LocalExecution.Before;
        }
    }

    private static void TriggerRpcCall(string id, RegisteredRpc rpc, object[] args)
    {
        var data = new List<string>();

        var parameters = rpc.OriginalMethod.GetParameters().ToList();
        
        var senderParameterIndex = parameters.FindIndex(IsSenderParameter);
        var receiverParameterIndex = parameters.FindIndex(IsReceiverParameter);
        
        var sender = PlayerControl.LocalPlayer;
        var receiverId = -1;

        for (var i = 0; i < args.Length; i++)
        {
            if (i == senderParameterIndex)
            {
                sender = (PlayerControl)args[i];
                continue;
            }

            if (i == receiverParameterIndex)
            {
                receiverId = ((PlayerControl)args[i]).OwnerId;
                continue;
            }

            data.Add(JsonSerializer.Serialize(args[i]));
        }

        new RpcData { Id = id, Data = data }.Send(sender, receiverId);
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
            if (rpc.Attribute.Execution != LocalExecution.None) return;
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
            else if (IsReceiverParameter(p))
            {
                args.Add(PlayerControl.LocalPlayer);
            }
            else
            {
                index++;
                try
                {
                    var deserializerMethod = typeof(JsonSerializer).GetMethods().FirstOrDefault(x =>
                        x.Name == nameof(JsonSerializer.Deserialize) && x.ContainsGenericParameters &&
                        x.GetParameters().Length == 2 && x.GetParameters()[0].ParameterType == typeof(string) &&
                        x.GetParameters()[1].ParameterType == typeof(JsonSerializerOptions));
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
        if (parameter.Name != SenderParameterName) return false;
        return parameter.ParameterType == SenderParameterType;
    }
    
    private static bool IsReceiverParameter(ParameterInfo parameter)
    {
        if (parameter.Name != ReceiverParameterName) return false;
        return parameter.ParameterType == ReceiverParameterType;
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

    internal void Send(PlayerControl sender = null, int receiverId = -1)
    {
        if (!AmongUsClient.Instance || !PlayerControl.LocalPlayer) return;
        sender = sender ? sender : PlayerControl.LocalPlayer;
        var writer = AmongUsClient.Instance.StartRpcImmediately(sender.NetId,
            RpcManager.ReservedRpcCallId, SendOption.Reliable, receiverId);
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

    public RpcAttribute(LocalExecution execution = LocalExecution.Before)
    {
        Execution = execution;
    }
}

public enum LocalExecution
{
    None,
    Before,
}