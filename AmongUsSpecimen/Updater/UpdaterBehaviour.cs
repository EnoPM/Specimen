using System;
using System.Collections.Generic;
using AmongUsSpecimen.UI;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;

namespace AmongUsSpecimen.Updater;

internal class UpdaterBehaviour : MonoBehaviour
{
    private void Update()
    {
        if (!UiManager.IsReady || Specimen.UpdateRequests.Count == 0) return;
        var cache = new List<UpdateModConfig>(Specimen.UpdateRequests);
        foreach (var config in cache)
        {
            var updater = new AutoUpdatedMod(config);
            Specimen.AutoUpdatedMods.Add(updater);
            this.StartCoroutine(updater.CoStart());
            Specimen.UpdateRequests.Remove(config);
        }
    }
}