using System.Collections.Generic;

namespace AmongUsSpecimen.ModOptions;

public sealed class ModOptionPreset
{
    public bool IsSharable { get; set; }
    public string Name { get; set; }
    public string VanillaOptions { get; set; }
    public Dictionary<int, int> Values { get; set; }
}