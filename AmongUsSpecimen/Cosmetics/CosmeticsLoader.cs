using System;
using System.IO;
using System.Text.Json;
using BepInEx.Unity.IL2CPP.Utils;
using System.Collections;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.UI.Components;
using UnityEngine;
using UnityEngine.Networking;

namespace AmongUsSpecimen.Cosmetics;

public class CosmeticsLoader : MonoBehaviour
{
    private bool isRunning;

    public void FetchCosmetics(string repository, string manifestFileName)
    {
        if (isRunning) return;
        this.StartCoroutine(CoFetchCosmetics(repository, manifestFileName));
    }

    private IEnumerator CoFetchCosmetics(string repository, string manifestFileName)
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

        yield return CoDownloadHats(response, repository);

        isRunning = false;
    }

    private static IEnumerator CoDownloadHats(ManifestFile response, string repository)
    {
        while (NotificationManager.Window == null)
        {
            yield return new WaitForEndOfFrame();
        }
        
        var hatsDirectory = CustomCosmeticsManager.HatsDirectory;
        if (!Directory.Exists(hatsDirectory)) Directory.CreateDirectory(hatsDirectory);
        CustomCosmeticsManager.UnregisteredHats.AddRange(CustomCosmeticsManager.SanitizeHats(response));
        var toDownload = CustomCosmeticsManager.GenerateDownloadList(CustomCosmeticsManager.UnregisteredHats, out var totalFileCount);
        Specimen.Instance.Log.LogMessage($"{toDownload.Count} hat asset files to download");
        if (toDownload.Count > 0)
        {
            var notification = NotificationManager.AddNotification(new DownloadListNotification(
                totalFileCount,
                totalFileCount - toDownload.Count,
                "Custom cosmetics update"));
            notification.UpdateProgression();
            foreach (var fileName in toDownload)
            {
                notification.IncrementDownloadFile();
                while (notification.PauseDownload)
                {
                    yield return new WaitForEndOfFrame();
                }
                yield return CoDownloadAssetFile($"https://raw.githubusercontent.com/{repository}/master/hats/{Uri.EscapeDataString(fileName)}", Path.Combine(hatsDirectory, fileName));
                CustomCosmeticsManager.OnHatFileDownloaded(fileName);
            }

            yield return new WaitForSeconds(2f);
            notification.Remove();
        }
        
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