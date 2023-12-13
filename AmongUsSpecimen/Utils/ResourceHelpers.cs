using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

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

    private static readonly Dictionary<string, Sprite> _sprites = new();
    
    public static Sprite LoadSpriteFromResources(this Assembly assembly, string path, float pixelsPerUnit)
    {
        var cacheKey = $"{path}{pixelsPerUnit}";
        if (_sprites.TryGetValue(cacheKey, out var sprite)) return sprite;
        var texture = assembly.LoadTextureFromResources(path);
        sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
        return _sprites[cacheKey] = sprite;
    }
    
    private static unsafe Texture2D LoadTextureFromResources(this Assembly assembly, string path)
    {
        var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
        var stream = assembly.GetManifestResourceStream(path);
        if (stream == null) return null;
        var length = stream.Length;
        var bytes = new Il2CppStructArray<byte>(length);
        _ = stream.Read(new Span<byte>(IntPtr.Add(bytes.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
        ImageConversion.LoadImage(texture, bytes, false);
        return texture;
    }

    internal static bool TranslationsInitialized => DestroyableSingleton<TranslationController>.InstanceExists &&
                                                  DestroyableSingleton<TranslationController>.Instance
                                                      .currentLanguage != null;
}