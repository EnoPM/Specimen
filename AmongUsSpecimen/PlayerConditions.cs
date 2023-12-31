﻿namespace AmongUsSpecimen;

public static class PlayerConditions
{
    public static bool CanUseVentButtonKeyBind(PlayerControl player)
    {
        return player.Data?.Role != null && player.Data.Role.IsImpostor;
    }

    public static bool CanUseKillButtonKeyBind(PlayerControl player)
    {
        return player.Data?.Role != null && player.Data.Role.IsImpostor;
    }

    public static bool AmHost()
    {
        return AmongUsClient.Instance && AmongUsClient.Instance.AmHost;
    }

    public static bool AmHostOrNotInGame()
    {
        return !AmongUsClient.Instance || AmHost();
    }
}