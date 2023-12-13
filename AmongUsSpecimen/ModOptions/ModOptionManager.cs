using System.Collections.Generic;
using System.Reflection;
using AmongUsSpecimen.Utils;

namespace AmongUsSpecimen.ModOptions;

internal static class ModOptionManager
{
    internal static readonly List<ModOptionTab> Tabs = [];
    internal static readonly List<BaseModOption> Options = [];
    internal static PresetManagerWindow PresetManagerWindow { get; set; }

    internal static void RegisterAssembly(Assembly assembly)
    {
        var results = assembly.GetClassesByAttribute<ModOptionContainerAttribute>();
        foreach (var item in results)
        {
            if (item.Attribute.ContainerType == ContainerType.Options) continue;
            if (!item.Type.IsClass || !item.Type.IsAbstract || !item.Type.IsSealed)
            {
                Specimen.Instance.Log.LogWarning($"Unable to load {item.Type.FullName} because is not a static type");
                continue;
            }

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(item.Type.TypeHandle);
        }

        foreach (var item in results)
        {
            if (item.Attribute.ContainerType == ContainerType.Tabs) continue;
            if (!item.Type.IsClass || !item.Type.IsAbstract || !item.Type.IsSealed)
            {
                Specimen.Instance.Log.LogWarning($"Unable to load {item.Type.FullName} because is not a static type");
                continue;
            }

            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(item.Type.TypeHandle);
        }
    }

    [Rpc(LocalExecution.None)]
    internal static void RpcSetOptionSelection(int id, int selection)
    {
        SetOptionSelection(id, selection);
    }

    [Rpc(LocalExecution.None)]
    private static void RpcBulkSetOptionSelection(PlayerControl __receiver, Dictionary<int, int> values)
    {
        foreach (var (id, selection) in values)
        {
            SetOptionSelection(id, selection);
        }
    }

    internal static void SendCurrentPresetTo(PlayerControl receiver)
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost) return;
        var values = new Dictionary<int, int>();
        foreach (var globalSelection in OptionStorage.Current.Global)
        {
            values[globalSelection.Key] = globalSelection.Value;
        }

        foreach (var presetSelection in OptionStorage.Current.GetCurrentPreset().Values)
        {
            values[presetSelection.Key] = presetSelection.Value;
        }

        RpcBulkSetOptionSelection(receiver, values);
    }

    internal static void RecreateCustomTabs()
    {
        var activeTab = Tabs.Find(x => x.SettingsGameObject && x.SettingsGameObject.active);
        ModOptionUtility.DestroyCustomOptionTabs();
        ModOptionUtility.CreateCustomOptionTabs();
        if (activeTab == null) return;
        if (!activeTab.TabPositionObject) return;
        var button = activeTab.TabPositionObject.GetComponentInChildren<PassiveButton>();
        if (!button) return;
        button.OnClick.Invoke();
    }

    private static void SetOptionSelection(int optionId, int selection)
    {
        var option = Options.Find(x => x.Id == optionId);
        if (option == null)
        {
            Specimen.Instance.Log.LogWarning($"SetOptionSelection: unknown option id {optionId}");
            return;
        }

        option.CurrentSelection = selection;
    }
}