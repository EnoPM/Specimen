﻿using System;
using System.IO;
using System.Text.Json;
using BepInEx.Unity.IL2CPP.Utils;
using System.Collections;
using System.Linq;
using AmongUsSpecimen.Extensions;
using AmongUsSpecimen.Resources;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.UI.Components;
using AmongUsSpecimen.VersionCheck;
using UnityEngine;
using UnityEngine.Networking;

namespace AmongUsSpecimen.Cosmetics;

public class CosmeticsLoader : MonoBehaviour
{
    private bool isRunning;

    private void Update()
    {
        if (InputManager.GetKeyDown(KeyCode.F12))
        {
            var allHandshakes = JsonSerializer.Serialize(VersionHandshakeManager.AllHandshakes);
            var playerOwnerIds = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var pc in PlayerControl.AllPlayerControls)
            {
                if (!pc || pc.Data == null) continue;
                playerOwnerIds[pc.Data.PlayerName] = pc.OwnerId;
            }
            
            Specimen.Instance.Log.LogMessage("*******************************************************");
            Specimen.Instance.Log.LogMessage($"{JsonSerializer.Serialize(playerOwnerIds)}");
            Specimen.Instance.Log.LogMessage($"{allHandshakes}");
            Specimen.Instance.Log.LogMessage("*******************************************************");
        }
    }

    public void FetchCosmetics(string repository, string manifestFileName, string customDirectory)
    {
        if (isRunning || customDirectory == null) return;
        this.StartCoroutine(CoFetchCosmetics(repository, manifestFileName, customDirectory));
    }

    private IEnumerator CoFetchCosmetics(string repository, string manifestFileName, string customDirectory)
    {
        isRunning = true;
        
        var url = $"https://raw.githubusercontent.com/{repository}/master/{manifestFileName}";
        var www = new UnityWebRequest();
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        Specimen.Instance.Log.LogMessage($"Download manifest at: {url}");
        www.SetUrl(url);
        www.downloadHandler = new DownloadHandlerBuffer();
        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            Specimen.Instance.Log.LogError(www.error);
            yield break;
        }

        var response = JsonSerializer.Deserialize<ManifestFile>(www.downloadHandler.text, new JsonSerializerOptions
        {
            AllowTrailingCommas = true
        });
        www.downloadHandler.Dispose();
        www.Dispose();

        yield return CoDownloadHats(response, repository, customDirectory);

        isRunning = false;
    }

    private static IEnumerator CoDownloadHats(ManifestFile response, string repository, string hatsDirectory)
    {
        while (NotificationManager.Window == null)
        {
            yield return new WaitForEndOfFrame();
        }
        if (hatsDirectory == null) yield break;
        if (!Directory.Exists(hatsDirectory)) Directory.CreateDirectory(hatsDirectory);
        CustomCosmeticsManager.UnregisteredHats.AddRange(CustomCosmeticsManager.SanitizeHats(response, hatsDirectory));
        var toDownload = CustomCosmeticsManager.GenerateDownloadList(CustomCosmeticsManager.UnregisteredHats, hatsDirectory, out var totalFileCount);
        Specimen.Instance.Log.LogMessage($"{toDownload.Count} hat asset files to download");
        if (toDownload.Count <= 0) yield break;
        var notification = NotificationManager.AddNotification(new DownloadListNotification(
            totalFileCount,
            totalFileCount - toDownload.Count,
            Translation.ResourceManager.GetString("CustomCosmeticsDownloadNotificationTitle")));
        notification.UpdateProgression();
        foreach (var fileName in toDownload)
        {
            while (notification.PauseDownload)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return CoDownloadAssetFile($"https://raw.githubusercontent.com/{repository}/master/hats/{Uri.EscapeDataString(fileName)}", Path.Combine(hatsDirectory, fileName));
            notification.IncrementDownloadFile();
            CustomCosmeticsManager.OnHatFileDownloaded(fileName);
        }

        yield return new WaitForSeconds(2f);
        notification.Remove();

    }

    private static IEnumerator CoDownloadAssetFile(string fileUrl, string destination)
    {
        var www = new UnityWebRequest();
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        www.SetUrl(fileUrl);
        www.downloadHandler = new DownloadHandlerBuffer();
        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            Specimen.Instance.Log.LogError($"{www.error}: {fileUrl}");
            yield break;
        }
        
        var persistTask = File.WriteAllBytesAsync(destination, www.downloadHandler.data);
        while (!persistTask.IsCompleted)
        {
            if (persistTask.Exception != null)
            {
                Specimen.Instance.Log.LogError(persistTask.Exception.Message);
                break;
            }

            yield return new WaitForEndOfFrame();
        }

        www.downloadHandler.Dispose();
        www.Dispose();
    }
}