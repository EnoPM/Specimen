using System.Collections.Generic;
using System.Linq;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.VersionCheck;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;

namespace AmongUsSpecimen.Updater;

internal class UpdaterBehaviour : MonoBehaviour
{
    private void Update()
    {
        if(!UiManager.IsReady) return;
        if (CustomKeyBinds.GetKeyboardButtonDown(Specimen.ToggleSpecimenDashboardActionName))
        {
            VersionHandshakeManager.Window?.Toggle();
        }
        if (ModUpdaterManager.UpdateRequests.Count == 0) return;
        var cache = new List<ModUpdaterConfig>(ModUpdaterManager.UpdateRequests);
        foreach (var config in cache)
        {
            var updater = new AutoUpdatedMod(config);
            ModUpdaterManager.AutoUpdatedMods.Add(updater);
            this.StartCoroutine(updater.CoStart());
            ModUpdaterManager.UpdateRequests.Remove(config);
        }
    }
}