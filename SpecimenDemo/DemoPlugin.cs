using BepInEx;
using BepInEx.Unity.IL2CPP;
using AmongUsSpecimen;
using AmongUsSpecimen.Cosmetics;
using AmongUsSpecimen.Updater;

namespace SpecimenDemo;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency(Specimen.Guid)]
[CustomRegion("Specimen", "specimen.eno.pm", "https://specimen.eno.pm", color: "#ff00ff")]
[CustomCosmetics("EnoPM/BetterOtherHats", "CustomHats.json")]
[ModUpdater("EnoPM/Specimen", Version, "DemoPlugin.dll")]
public class DemoPlugin : BasePlugin
{
    private const string Guid = "demo.specimen.eno.pm";
    private const string Name = "DemoSpecimen";
    private const string Version = "0.0.2";
    
    public override void Load()
    {
        // Plugin startup logic
        AddComponent<KeyBindsBehaviour>();
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}