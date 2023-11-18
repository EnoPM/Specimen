using System.Collections;
using Il2CppSystem;
using UnityEngine;

namespace AmongUsSpecimen.Extensions;

public static class PlayerControlExtensions
{
    public static void RpcTeleportTo(this PlayerControl __sender, PlayerControl target)
    {
        var pos = target.transform.position;
        __sender.RpcTeleportTo(pos);
    }

    [Rpc]
    public static void RpcTeleportTo(this PlayerControl __sender, Vector2 position)
    {
        var net = __sender.NetTransform;
        var minSid = (ushort)(net.lastSequenceId + 10);
        if (!NetHelpers.SidGreaterThan(minSid, net.lastSequenceId)) return;
        if (net.IsInMiddleOfAnimationThatMakesPlayerInvisible())
        {
            net.tempSnapPosition = new Nullable<Vector2>(position);
        }
        else
        {
            net.ClearPositionQueues();
            net.lastSequenceId = minSid;
            net.tempSnapPosition = new Nullable<Vector2>();
            net.transform.position = net.body.position = position;
            net.body.velocity = Vector2.zero;
        }
    }

    public static SpriteRenderer GetSpriteRenderer(this PlayerControl player)
    {
        if (!player || !player.cosmetics) return null;
        return player.cosmetics.currentBodySprite.BodySprite;
    }

    public static IEnumerator CoMorphTo(this PlayerControl player, PlayerControl target, float duration, PlayerOutfitType outfitType)
    {
        if (!player.cosmetics || !target.cosmetics || player.CurrentOutfitType != PlayerOutfitType.Default || target.CurrentOutfitType != PlayerOutfitType.Default || player.Data == null || target.Data == null) yield break;
        var fadeOutDuration = duration / 2f;
        var fadeInDuration = duration - fadeOutDuration;
        var playerBaseColor = Palette.PlayerColors[player.Data.DefaultOutfit.ColorId];
        var targetBaseColor = Palette.PlayerColors[target.Data.DefaultOutfit.ColorId];
        for (var i = 0f; i < fadeOutDuration; i += Time.deltaTime)
        {
            var p = i / fadeOutDuration;
            var alpha = 1f - p;
            var playerColor = Color.Lerp(playerBaseColor, Color.black, p);
            PlayerMaterial.SetColors(playerColor, player.GetSpriteRenderer());
            player.SetHatAndVisorAlpha(alpha);
            var pet = player.GetPet();
            if (pet)
            {
                var petColor = pet.rend.color;
                petColor.a = alpha;
                pet.rend.color = petColor;
            }
            var color = player.cosmetics.skin.layer.color;
            color.a = alpha;
            player.cosmetics.skin.layer.color = color;
            var nameColor = player.cosmetics.nameText.color;
            color.a = alpha;
            player.cosmetics.nameText.color = nameColor;
            yield return new WaitForEndOfFrame();
        }
        player.RawSetOutfit(target.Data.DefaultOutfit, outfitType);
        player.RawSetName(target.Data.PlayerName);
        for (var i = 0f; i < fadeInDuration; i += Time.deltaTime)
        {
            var p = i / fadeInDuration;
            var playerColor = Color.Lerp(Color.black, targetBaseColor, p);
            PlayerMaterial.SetColors(playerColor, player.GetSpriteRenderer());
            player.SetHatAndVisorAlpha(p);
            var pet = player.GetPet();
            if (pet)
            {
                var petColor = pet.rend.color;
                petColor.a = p;
                pet.rend.color = petColor;
            }
            var color = player.cosmetics.skin.layer.color;
            color.a = p;
            player.cosmetics.skin.layer.color = color;
            var nameColor = player.cosmetics.nameText.color;
            color.a = p;
            player.cosmetics.nameText.color = nameColor;
            yield return new WaitForEndOfFrame();
        }
        player.RawSetColor(target.Data.DefaultOutfit.ColorId);
    }

    public static IEnumerator CoMorph(this PlayerControl player, PlayerControl target, float duration)
    {
        yield return player.CoMorphTo(target, 2f, PlayerOutfitType.Default);
        yield return new WaitForSeconds(duration);
        yield return player.CoMorphTo(player, 2f, PlayerOutfitType.Default);
    }
}