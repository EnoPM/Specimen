using System.Collections.Generic;

namespace AmongUsSpecimen.Options;

public sealed class CustomOptionPreset
{
    public bool IsSharable { get; set; }
    public string Name { get; set; }
    public Dictionary<int, int> Values { get; set; }
}