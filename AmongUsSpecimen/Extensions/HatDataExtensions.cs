using AmongUsSpecimen.Cosmetics;

namespace AmongUsSpecimen.Extensions;

internal static class HatDataExtensions
{
    public static HatExtension GetHatExtension(this HatData hat)
    {
        if (CustomCosmeticsManager.TestHatExtension != null && CustomCosmeticsManager.TestHatExtension.Condition.Equals(hat.name))
        {
            return CustomCosmeticsManager.TestHatExtension;
        }

        return CustomCosmeticsManager.HatExtensionCache.TryGetValue(hat.name, out var extension) ? extension : null;
    }
}