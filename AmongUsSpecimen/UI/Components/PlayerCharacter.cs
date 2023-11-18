using UnityEngine;
using UnityEngine.UI;

namespace AmongUsSpecimen.UI.Components;

public class PlayerCharacter
{
    private const int Size = 50;
    
    public readonly GameObject Parent;
    public readonly GameObject ContentRoot;
    public readonly Image Body;

    private readonly GameObject _cacheContainer;
    private SpriteRenderer _cache;

    public PlayerCharacter(GameObject parent)
    {
        Parent = parent;
        ContentRoot = UiFactory.CreateHorizontalGroup(Parent, "PlayerCharacter", false, false, false, false, bgColor: Palette.EnabledColor);
        UiFactory.SetLayoutElement(ContentRoot, Mathf.RoundToInt(0.8f * Size), Size, 0, 0, 0, 0);
        Body = ContentRoot.GetComponent<Image>();
        _cacheContainer = UiFactory.CreateUIObject("CacheContainer", ContentRoot, Vector2.zero);
    }

    public void SetPlayer(PlayerControl player)
    {
        if (!player || !player.cosmetics) return;
        if (!_cache)
        {
            _cache = Object.Instantiate(player.cosmetics.normalBodySprite.BodySprite, _cacheContainer.transform);
        }
        var material = _cache.material;
        PlayerMaterial.SetColors(player.cosmetics.ColorId, _cache);
        Body.material = material;
        Body.sprite = _cache.sprite;
        Body.color = Palette.EnabledColor;
    }
}