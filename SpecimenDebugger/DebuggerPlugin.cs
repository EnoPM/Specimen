using AmongUsSpecimen;
using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace SpecimenDebugger;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency(Specimen.Guid)]
public sealed class DebuggerPlugin : BasePlugin
{
    private const string Guid = "debugger.specimen.eno.pm";
    private const string Name = "SpecimenDebugger";
    private const string Version = "0.0.1";
    
    public override void Load()
    {
        // Plugin startup logic
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}