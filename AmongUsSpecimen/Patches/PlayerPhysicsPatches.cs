using AmongUsSpecimen.Cosmetics;
using AmongUsSpecimen.Extensions;
using HarmonyLib;

namespace AmongUsSpecimen.Patches;

[HarmonyPatch(typeof(PlayerPhysics))]
internal static class PlayerPhysicsPatches
{
    [HarmonyPatch(nameof(PlayerPhysics.HandleAnimation))]
    [HarmonyPostfix]
    private static void HandleAnimationPostfix(PlayerPhysics __instance)
    {
        var currentAnimation = __instance.Animations.Animator.GetCurrentAnimation();
        if (currentAnimation == __instance.Animations.group.ClimbUpAnim) return;
        if (currentAnimation == __instance.Animations.group.ClimbDownAnim) return;
        var hatParent = __instance.myPlayer.cosmetics.hat;
        if (hatParent == null || hatParent == null) return;
        if (!hatParent.TryGetCached(out var viewData)) return;
        var extend = hatParent.Hat.GetHatExtension();
        if (extend == null) return;
        if (extend.FlipImage != null)
        {
            hatParent.FrontLayer.sprite = __instance.FlipX ? extend.FlipImage : viewData.MainImage;
        }

        if (extend.BackFlipImage == null) return;
        if (__instance.FlipX)
        {
            hatParent.FrontLayer.sprite = extend.FlipImage;
        }
        else
        {
            hatParent.BackLayer.sprite = viewData.BackImage;
        }
    }
}