using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using AmongUsSpecimen;

namespace SpecimenDemo;

[BepInPlugin(Guid, Name, Version)]
[BepInDependency(Specimen.Guid)]
public class DemoPlugin : BasePlugin
{
    private const string Guid = "demo.specimen.eno.pm";
    private const string Name = "DemoSpecimen";
    private const string Version = "0.0.1";
    
    public override void Load()
    {
        // Plugin startup logic
        RpcManager.LoadAssembly(Assembly.GetExecutingAssembly());
        AddComponent<KeyBindsBehaviour>();
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}