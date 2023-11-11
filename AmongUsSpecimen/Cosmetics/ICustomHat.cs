namespace AmongUsSpecimen.Cosmetics;

public interface ICustomHat
{
    public string Author { get; set; }
    public string Name { get; set; }
    public string Package { get; set; }
    public string Condition { get; set; }
    public bool Bounce { get; set; }
    public bool Adaptive { get; set; }
    public bool Behind { get; set; }
    public string Resource { get; set; }
    public string Resource_Hash { get; set; }
    public string BackResource { get; set; }
    public string BackResource_Hash { get; set; }
    public string FlipResource { get; set; }
    public string FlipResource_Hash { get; set; }
    public string BackFlipResource { get; set; }
    public string BackFlipResource_Hash { get; set; }
    public string ClimbResource { get; set; }
    public string ClimbResource_Hash { get; set; }
}