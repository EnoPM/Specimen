using System.IO;
using UnityEngine;
using xCloud;

namespace AmongUsSpecimen.Utils;

public static class ResourceHelpers
{
    public static Texture2D LoadTextureFromPath(string path)
    {
        if (!File.Exists(path)) return null;
        var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
        try
        {
            var byteTexture = Il2CppSystem.IO.File.ReadAllBytes(path);
            ImageConversion.LoadImage(texture, byteTexture, false);
        }
        catch
        {
            Specimen.Instance.Log.LogError("Error loading texture from disk: " + path);
            return null;
        }

        return texture;
    }
}