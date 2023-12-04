using System;

namespace AmongUsSpecimen;

public static class GameEventManager
{
    public static event Action GameStarted;
    internal static void TriggerGameStarted() => GameStarted?.Invoke();

    public static event Action GameEnded;
    internal static void TriggerGameEnded() => GameEnded?.Invoke();

    public static event Action MeetingStarted;
    internal static void TriggerMeetingStarted() => MeetingStarted?.Invoke();

    public static event Action<PlayerControl> MeetingEnded;
    internal static void TriggerMeetingEnded(PlayerControl exiled) => MeetingEnded?.Invoke(exiled);

    public static event Action<PlayerControl> PlayerJoined;
    internal static void TriggerPlayerJoined(PlayerControl player) => PlayerJoined?.Invoke(player);
    
    public static event Action<PlayerControl> PlayerLeft;
    internal static void TriggerPlayerLeft(PlayerControl player) => PlayerLeft?.Invoke(player);

    public static event Action HostChanged;
    internal static void TriggerHostChanged() => HostChanged?.Invoke();
}