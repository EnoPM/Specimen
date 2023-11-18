using System.Reflection;
using AmongUsSpecimen.Utils;
using UnityEngine;

namespace AmongUsSpecimen;

internal static class SpecimenSprites
{
    internal static readonly Sprite SuccessIcon = FromResources("SuccessIcon");
    internal static readonly Sprite ErrorIcon = FromResources("ErrorIcon");
    internal static readonly Sprite WarningIcon = FromResources("WarningIcon");
    internal static readonly Sprite UiBackgroundBase = FromResources("UiBackgroundBase", 115f);
    internal static readonly Sprite ModSettingsTabIcon = FromResources("ModSettingsTabIcon", 400f);
    internal static readonly Sprite ZoomInButton = FromResources("ZoomInButton", 1f);
    internal static readonly Sprite ZoomOutButton = FromResources("ZoomOutButton", 1f);
    
    private static Sprite FromResources(string fileName, float pixelsPerUnits = 1f)
    {
        return Assembly.GetExecutingAssembly().LoadSpriteFromResources($"AmongUsSpecimen.Resources.Sprites.{fileName}.png", pixelsPerUnits);
    }
}