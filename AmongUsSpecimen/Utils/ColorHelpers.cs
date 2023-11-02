using UnityEngine;

namespace AmongUsSpecimen.Utils;

public static class ColorHelpers
{
    public static string Colorize(Color c, string s) {
        return $"<color=#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}{ToByte(c.a):X2}>{s}</color>";
    }
    
    private static byte ToByte(float f) {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
}