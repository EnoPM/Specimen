using Il2CppSystem;
using UnityEngine;

namespace AmongUsSpecimen.Extensions;

public static class PlayerControlExtensions
{
    public static void RpcTeleportTo(this PlayerControl __sender, PlayerControl target)
    {
        var pos = target.transform.position;
        __sender.RpcTeleportTo(pos);
    }

    [Rpc]
    public static void RpcTeleportTo(this PlayerControl __sender, Vector2 position)
    {
        var net = __sender.NetTransform;
        var minSid = (ushort)(net.lastSequenceId + 10);
        if (!NetHelpers.SidGreaterThan(minSid, net.lastSequenceId)) return;
        if (net.IsInMiddleOfAnimationThatMakesPlayerInvisible())
        {
            net.tempSnapPosition = new Nullable<Vector2>(position);
        }
        else
        {
            net.ClearPositionQueues();
            net.lastSequenceId = minSid;
            net.tempSnapPosition = new Nullable<Vector2>();
            net.transform.position = net.body.position = position;
            net.body.velocity = Vector2.zero;
        }
    }
}