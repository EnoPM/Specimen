using AmongUsSpecimen;

namespace SpecimenDemo;

public static class CustomPlayerControlRpc
{
    [Rpc(LocalExecution.None)]
    public static void RpcSetVisualName(this PlayerControl __sender, string name)
    {
        if (!__sender || !__sender.cosmetics || !__sender.cosmetics.nameText) return;
        __sender.cosmetics.nameText.text = name;
    }
}