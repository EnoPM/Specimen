using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using AmongUsSpecimen.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AmongUsSpecimen.Cosmetics;

internal static class CustomCosmeticsManager
{
    internal const string InnerslothHatPackageName = "Innersloth Hats";
    internal const string DeveloperHatPackageName = "Developer Hats";
    private static string CustomSkinsDirectory => Path.Combine(Specimen.ResourcesDirectory, "Cosmetics");
    internal static string HatsDirectory => Path.Combine(CustomSkinsDirectory, "Hats");
    
    internal static readonly List<CustomHat> UnregisteredHats = new();
    internal static readonly Dictionary<string, CustomHat> RegisteredHats = new();
    internal static readonly Dictionary<string, HatViewData> HatViewDataCache = new();
    private static readonly List<HatParent> HatParentCache = new();
    internal static readonly Dictionary<string, HatExtension> HatExtensionCache = new();
    private static readonly Dictionary<string, List<string>> HatFiles = new();
    
    private static readonly CosmeticsLoader Loader;
    private static Material cachedShader;
    
    internal static HatExtension TestHatExtension { get; private set; }

    static CustomCosmeticsManager()
    {
        Loader = Specimen.Instance.AddComponent<CosmeticsLoader>();
    }

    internal static void Cache(HatParent hatParent)
    {
        if (!hatParent || !hatParent.Hat)
        {
            Specimen.Instance.Log.LogWarning($"Trying to cache HatParent without HatData");
            return;
        }
        if (HatParentCache.Contains(hatParent))
        {
            Specimen.Instance.Log.LogWarning($"Trying to cache already cached HatParent");
            return;
        }
        HatParentCache.Add(hatParent);
    }
    
    internal static void RegisterAssembly(Assembly assembly)
    {
        var results = assembly.GetClassesByAttribute<CustomCosmeticsAttribute>();
        foreach (var result in results)
        {
            var repository = result.Attribute.Repository;
            var directory = result.Attribute.CustomDirectory != string.Empty
                ? Path.Combine(Specimen.AmongUsDirectory, result.Attribute.CustomDirectory)
                : HatsDirectory;
            Loader.FetchCosmetics(repository, result.Attribute.ManifestFileName, directory);
        }
    }
    
    internal static bool TryGetCached(this HatParent hatParent, out HatViewData asset)
    {
        if (hatParent && hatParent.Hat) return hatParent.Hat.TryGetCached(out asset);
        asset = null;
        return false;
    }
    
    private static bool TryGetCached(this HatData hat, out HatViewData asset)
    {
        return HatViewDataCache.TryGetValue(hat.name, out asset);
    }

    private static bool IsCached(this HatData hat)
    {
        return HatViewDataCache.ContainsKey(hat.name);
    }
    
    internal static bool IsCached(this HatParent hatParent)
    {
        return hatParent.Hat.IsCached();
    }
    
    internal static HatData CreateHatBehaviour(CustomHat ch, bool testOnly = false)
    {
        if (cachedShader == null) cachedShader = DestroyableSingleton<HatManager>.Instance.PlayerMaterial;

        var viewData = HatViewDataCache[ch.Name] = ScriptableObject.CreateInstance<HatViewData>();
        var hat = ScriptableObject.CreateInstance<HatData>();

        viewData.MainImage = CreateHatSprite(ch.Resource, ch.HatsDirectoryPath);
        viewData.FloorImage = viewData.MainImage;
        if (ch.BackResource != null)
        {
            viewData.BackImage = CreateHatSprite(ch.BackResource, ch.HatsDirectoryPath);
            // TODO: Check if next line is really needed and don't cause unexpected behaviour
            // viewData.LeftBackImage = viewData.BackImage;
            ch.Behind = true;
        }

        if (ch.ClimbResource != null)
        {
            viewData.ClimbImage = CreateHatSprite(ch.ClimbResource, ch.HatsDirectoryPath);
            viewData.LeftClimbImage = viewData.ClimbImage;
        }

        hat.name = ch.Name;
        hat.displayOrder = 99;
        hat.ProductId = "hat_" + ch.Name.Replace(' ', '_');
        hat.InFront = !ch.Behind;
        hat.NoBounce = !ch.Bounce;
        hat.ChipOffset = new Vector2(0f, 0.2f);
        hat.Free = true;

        if (ch.Adaptive && cachedShader != null)
        {
            viewData.AltShader = cachedShader;
        }
        
        var extend = new HatExtension
        {
            Author = ch.Author ?? "Unknown",
            Package = ch.Package ?? "Misc.",
            Condition = ch.Condition ?? "none"
        };

        if (ch.FlipResource != null)
        {
            extend.FlipImage = CreateHatSprite(ch.FlipResource, ch.HatsDirectoryPath);
        }

        if (ch.BackFlipResource != null)
        {
            extend.BackFlipImage = CreateHatSprite(ch.BackFlipResource, ch.HatsDirectoryPath);
        }

        if (testOnly)
        {
            TestHatExtension = extend;
            TestHatExtension.Condition = hat.name;
        }
        else
        {
            HatExtensionCache[hat.name] = extend;
        }
        
        hat.ViewDataRef = new AssetReference(HatViewDataCache[hat.name].Pointer);
        hat.CreateAddressableAsset();
        return hat;
    }
    
    private static Sprite CreateHatSprite(string path, string hatsDirectory)
    {
        var texture = ResourceHelpers.LoadTextureFromPath(Path.Combine(hatsDirectory, path));
        if (texture == null) return null;
        var sprite = Sprite.Create(texture, 
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.53f, 0.575f),
            texture.width * 0.375f);
        if (sprite == null) return null;
        texture.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;

        return sprite;
    }

    internal static List<CustomHat> CreateHatDetailsFromFileNames(IEnumerable<string> fileNames, bool fromDisk = false)
    {
        var fronts = new Dictionary<string, CustomHat>();
        var backs = new Dictionary<string, string>();
        var flips = new Dictionary<string, string>();
        var backFlips = new Dictionary<string, string>();
        var climbs = new Dictionary<string, string>();

        foreach (var fileName in fileNames)
        {
            var index = fileName.LastIndexOf("\\", StringComparison.InvariantCulture) + 1;
            var s = fromDisk ? fileName[index..].Split('.')[0] : fileName.Split('.')[3];
            var p = s.Split('_');
            var options = new HashSet<string>(p);
            if (options.Contains("back") && options.Contains("flip"))
            {
                backFlips[p[0]] = fileName;
            }
            else if (options.Contains("climb"))
            {
                climbs[p[0]] = fileName;
            }
            else if (options.Contains("back"))
            {
                backs[p[0]] = fileName;
            }
            else if (options.Contains("flip"))
            {
                flips[p[0]] = fileName;
            }
            else
            {
                fronts[p[0]] = new CustomHat
                {
                    Resource = fileName,
                    Name = p[0].Replace('-', ' '),
                    Bounce = options.Contains("bounce"),
                    Adaptive = options.Contains("adaptive"),
                    Behind = options.Contains("behind"),
                };
            }
        }

        var hats = new List<CustomHat>();

        foreach (var (k, hat) in fronts)
        {
            backs.TryGetValue(k, out var backResource);
            climbs.TryGetValue(k, out var climbResource);
            flips.TryGetValue(k, out var flipResource);
            backFlips.TryGetValue(k, out var backFlipResource);
            if (backResource != null) hat.BackResource = backResource;
            if (climbResource != null) hat.ClimbResource = climbResource;
            if (flipResource != null) hat.FlipResource = flipResource;
            if (backFlipResource != null) hat.BackFlipResource = backFlipResource;
            if (hat.BackResource != null) hat.Behind = true;
            hats.Add(hat);
        }

        return hats;
    }

    internal static IEnumerable<CustomHat> SanitizeHats(ManifestFile response, string hatsDirectory)
    {
        if (response.Hats == null) return [];
        foreach (var hat in response.Hats)
        {
            hat.Resource = SanitizeFileName(hat.Resource);
            hat.BackResource = SanitizeFileName(hat.BackResource);
            hat.ClimbResource = SanitizeFileName(hat.ClimbResource);
            hat.FlipResource = SanitizeFileName(hat.FlipResource);
            hat.BackFlipResource = SanitizeFileName(hat.BackFlipResource);
            hat.HatsDirectoryPath = hatsDirectory;
        }

        return response.Hats;
    }

    private static string SanitizeFileName(string path)
    {
        if (path == null || !path.EndsWith(".png")) return null;
        return path.Replace("\\", "")
            .Replace("/", "")
            .Replace("*", "")
            .Replace("..", "");
    }

    private static bool ResourceRequireDownload(string filePath, string resHash, HashAlgorithm algorithm)
    {
        if (resHash == null || !File.Exists(filePath))
        {
            return true;
        }
        using var stream = File.OpenRead(filePath);
        var hash = BitConverter.ToString(algorithm.ComputeHash(stream))
            .Replace("-", string.Empty)
            .ToLowerInvariant();
        var result = !resHash.Equals(hash);
        if (result)
        {
            Specimen.Instance.Log.LogMessage($"ResourceRequireDownload: {filePath} ({resHash} == {hash})");
        }
        return result;
    }

    private static bool HydrateHat(HatViewData viewData, CustomHat hat, string fileName)
    {
        var completed = new List<bool>();
        if (fileName == hat.Resource)
        {
            viewData.MainImage = CreateHatSprite(hat.Resource, hat.HatsDirectoryPath);
            viewData.FloorImage = viewData.MainImage;
        }

        if (hat.BackResource != null && fileName == hat.BackResource)
        {
            viewData.BackImage = CreateHatSprite(hat.BackResource, hat.HatsDirectoryPath);
            viewData.LeftBackImage = viewData.BackImage;
            hat.Behind = true;
        }

        if (hat.ClimbResource != null && fileName == hat.ClimbResource)
        {
            viewData.ClimbImage = CreateHatSprite(hat.ClimbResource, hat.HatsDirectoryPath);
            viewData.LeftClimbImage = viewData.ClimbImage;
        }
        

        completed.Add(viewData.MainImage && viewData.FloorImage);
        completed.Add(hat.BackResource == null || (viewData.BackImage && viewData.LeftBackImage));
        completed.Add(hat.ClimbResource == null || (viewData.ClimbImage && viewData.LeftClimbImage));
        
        return !completed.Contains(false);
    }

    private static void RefreshEquippedHats(string hatName)
    {
        var parents = HatParentCache.Where(x => x && x.Hat.name == hatName);
        foreach (var parent in parents)
        {
            parent.PopulateFromHatViewData();
        }
        
        var players = PlayerControl.AllPlayerControls.ToArray().Where(x =>
            x && x.Data != null && x.cosmetics && x.cosmetics.hat && x.cosmetics.hat.Hat &&
            x.cosmetics.hat.Hat.name == hatName);
        foreach (var player in players)
        {
            player.cosmetics.hat.PopulateFromHatViewData();
        }
    }

    internal static void OnHatFileDownloaded(string fileName)
    {
        var matches = HatFiles.Where(x => x.Value.Contains(fileName));
        var toRemove = new List<string>();
        foreach (var match in matches)
        {
            if (!RegisteredHats.TryGetValue(match.Key, out var hat)) continue;
            if (!HatViewDataCache.TryGetValue(match.Key, out var viewData)) continue;
            if (HydrateHat(viewData, hat, fileName))
            {
                toRemove.Add(match.Key);
                RefreshEquippedHats(hat.Name);
            }
        }

        foreach (var key in toRemove)
        {
            HatFiles.Remove(key);
        }
    }

    internal static List<string> GenerateDownloadList(List<CustomHat> hats, string hatsDirectory, out int totalFileCount)
    {
        totalFileCount = 0;
        var algorithm = MD5.Create();
        var toDownload = new List<string>();
        
        foreach (var hat in hats)
        {
            HatFiles[hat.Name] = new List<string>();
            var files = new List<Tuple<string, string>>
            {
                new(hat.Resource, hat.Resource_Hash),
                new(hat.BackResource, hat.BackResource_Hash),
                new(hat.ClimbResource, hat.ClimbResource_Hash),
                new(hat.FlipResource, hat.FlipResource_Hash),
                new(hat.BackFlipResource, hat.BackFlipResource_Hash)
            };
            foreach (var (fileName, fileHash) in files)
            {
                if (fileName == null) continue;
                totalFileCount++;
                if (!ResourceRequireDownload(Path.Combine(hatsDirectory, fileName), fileHash, algorithm)) continue;
                HatFiles[hat.Name].Add(fileName);
                toDownload.Add(fileName);
            }
        }

        return toDownload;
    }
}