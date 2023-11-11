using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using AmongUsSpecimen.Cosmetics;
using TMPro;
using UnityEngine;

namespace AmongUsSpecimen.Extensions;

public static class HatsTabExtensions
{
    private static TextMeshPro TextTemplate { get; set; }
    
    public static void SetupCustomHats(this HatsTab hatsTab)
    {
        for (var i = 0; i < hatsTab.scroller.Inner.childCount; i++)
        {
            UnityEngine.Object.Destroy(hatsTab.scroller.Inner.GetChild(i).gameObject);
        }

        hatsTab.ColorChips = new Il2CppSystem.Collections.Generic.List<ColorChip>();
        var unlockedHats = DestroyableSingleton<HatManager>.Instance.GetUnlockedHats();
        var packages = new Dictionary<string, List<Tuple<HatData, HatExtension>>>();

        foreach (var hatBehaviour in unlockedHats)
        {
            var ext = hatBehaviour.GetHatExtension();
            if (ext != null)
            {
                if (!packages.ContainsKey(ext.Package))
                {
                    packages[ext.Package] = new List<Tuple<HatData, HatExtension>>();
                }
                packages[ext.Package].Add(new Tuple<HatData, HatExtension>(hatBehaviour, ext));
            }
            else
            {
                if (!packages.ContainsKey(CustomCosmeticsManager.InnerslothHatPackageName))
                {
                    packages[CustomCosmeticsManager.InnerslothHatPackageName] = new List<Tuple<HatData, HatExtension>>();
                }
                packages[CustomCosmeticsManager.InnerslothHatPackageName].Add(new Tuple<HatData, HatExtension>(hatBehaviour, null));
            }
        }

        var yOffset = hatsTab.YStart;
        TextTemplate = GameObject.Find("HatsGroup").transform.FindChild("Text").GetComponent<TextMeshPro>();

        var orderedKeys = packages.Keys.OrderBy(x =>
            x switch
            {
                CustomCosmeticsManager.InnerslothHatPackageName => 1000,
                CustomCosmeticsManager.DeveloperHatPackageName => 0,
                _ => 500
            });
        foreach (var key in orderedKeys)
        {
            var value = packages[key];
            yOffset = hatsTab.CreatePackage(value, key, yOffset);
        }
        
        hatsTab.scroller.ContentYBounds.max = -(yOffset + 4.1f);
    }
    
    private static float CreatePackage(this HatsTab hatsTab, 
        List<Tuple<HatData, HatExtension>> hats, string packageName, float yStart)
    {
        var isDefaultPackage = CustomCosmeticsManager.InnerslothHatPackageName == packageName;
        if (!isDefaultPackage)
        {
            hats = hats.OrderBy(x => x.Item1.name).ToList();
        }

        var offset = yStart;
        if (TextTemplate != null)
        {
            var title = UnityEngine.Object.Instantiate(TextTemplate, hatsTab.scroller.Inner);
            title.transform.localPosition = new Vector3(2.25f, yStart, -1f);
            title.transform.localScale = Vector3.one * 1.5f;
            title.fontSize *= 0.5f;
            title.enableAutoSizing = false;
            hatsTab.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(_ => { title.SetText(packageName); })));
            offset -= 0.8f * hatsTab.YOffset;
        }

        for (var i = 0; i < hats.Count; i++)
        {
            var (hat, ext) = hats[i];
            var xPos = hatsTab.XRange.Lerp(i % hatsTab.NumPerRow / (hatsTab.NumPerRow - 1f));
            var yPos = offset - Mathf.RoundToInt(i / (float)hatsTab.NumPerRow) * (isDefaultPackage ? 1f : 1.5f) * hatsTab.YOffset;
            var colorChip = UnityEngine.Object.Instantiate(hatsTab.ColorTabPrefab, hatsTab.scroller.Inner);
            if (ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard)
            {
                colorChip.Button.OnMouseOver.AddListener((Action)(() => hatsTab.SelectHat(hat)));
                colorChip.Button.OnMouseOut.AddListener((Action)(() => hatsTab.SelectHat(DestroyableSingleton<HatManager>.Instance.GetHatById(DataManager.Player.Customization.Hat))));
                colorChip.Button.OnClick.AddListener((Action)hatsTab.ClickEquip);
            }
            else
            {
                colorChip.Button.OnClick.AddListener((Action)(() => hatsTab.SelectHat(hat)));
            }
            colorChip.Button.ClickMask = hatsTab.scroller.Hitbox;
            colorChip.Inner.SetMaskType(PlayerMaterial.MaskType.ScrollingUI);
            hatsTab.UpdateMaterials(colorChip.Inner.FrontLayer, hat);
            var background = colorChip.transform.FindChild("Background");
            var foreground = colorChip.transform.FindChild("ForeGround");

            if (ext != null)
            {
                if (background != null) {
                    background.localPosition = Vector3.down * 0.243f;
                    var localScaleCache = background.localScale;
                    background.localScale = new Vector3(localScaleCache.x, 0.8f, localScaleCache.y);
                }
                if (foreground != null) {
                    foreground.localPosition = Vector3.down * 0.243f;
                }
                
                if (TextTemplate != null) {
                    var description = UnityEngine.Object.Instantiate(TextTemplate, colorChip.transform);
                    description.transform.localPosition = new Vector3(0f, -0.65f, -1f);
                    description.alignment = TextAlignmentOptions.Center;
                    description.transform.localScale = Vector3.one * 0.65f;
                    hatsTab.StartCoroutine(Effects.Lerp(0.1f, new Action<float>(_ => { description.SetText($"{hat.name}\nby {ext.Author}"); })));
                }
            }
            
            colorChip.transform.localPosition = new Vector3(xPos, yPos, -1f);
            colorChip.Inner.SetHat(hat, hatsTab.HasLocalPlayer() ? PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId : DataManager.Player.Customization.Color);
            colorChip.Inner.transform.localPosition = hat.ChipOffset;
            colorChip.Tag = hat;
            colorChip.SelectionHighlight.gameObject.SetActive(false);
            hatsTab.ColorChips.Add(colorChip);
            CustomCosmeticsManager.Cache(colorChip.Inner);
        }

        return offset - (hats.Count - 1) / (float)hatsTab.NumPerRow * (isDefaultPackage ? 1f : 1.5f) * hatsTab.YOffset -
               1.75f;
    }
}