using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using AmongUsSpecimen.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace AmongUsSpecimen.Updater;

public class AutoUpdatedMod
{
    public bool HasUpdateAvailable => LatestRelease != null;
    
    public readonly UpdateModConfig Config;
    private UpdaterWindow _window;
    public List<GithubRelease> Releases;
    public GithubRelease LatestRelease { get; private set; }

    public AutoUpdatedMod(UpdateModConfig config)
    {
        Config = config;
    }

    public IEnumerator CoStart()
    {
        Releases = null;
        yield return CoLoadReleases();
        if (Releases == null) yield break;
        _window = UiManager.RegisterWindow<UpdaterWindow>(Config.VersionToCompare, Releases, LatestRelease);
    }

    public IEnumerator CoLoadReleases()
    {
        var www = MakeWebRequest(
            UnityWebRequest.UnityWebRequestMethod.Get,
            $"https://api.github.com/repos/{Config.RepositoryOwner}/{Config.RepositoryName}/releases");
        var operation = www.SendWebRequest();
        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            LoadReleasesError(www.error);
            yield break;
        }

        Releases = JsonSerializer.Deserialize<List<GithubRelease>>(www.downloadHandler.text);
        www.downloadHandler.Dispose();
        www.Dispose();
        Releases.Sort(CompareReleases);
        var latestRelease = Releases.FirstOrDefault();
        if (latestRelease != null && latestRelease.Version != Config.VersionToCompare)
        {
            NewUpdateAvailable(latestRelease);
        }
    }
    
    public IEnumerator CoDownloadRelease(GithubRelease release)
    {
        foreach (var asset in release.Assets)
        {
            if (!IsUpdateAsset(asset)) continue;
            yield return CoDownloadAsset(asset);
        }
    }
    
    private IEnumerator CoDownloadAsset(GithubAsset asset)
    {
        var www = MakeWebRequest(UnityWebRequest.UnityWebRequestMethod.Get, asset.DownloadUrl);
        var operation = www.SendWebRequest();
        
        DownloadAssetStart(asset);

        while (!operation.isDone)
        {
            DownloadAssetProgress(asset, www.downloadProgress);
            yield return new WaitForEndOfFrame();
        }
        
        if (www.isNetworkError || www.isHttpError)
        {
            DownloadAssetError(asset, www.error);
            yield break;
        }
        
        DownloadAssetEnd(asset);

        CopyAssetStart(asset);
        
        var filePath = Path.Combine(Config.Directory, asset.Name);
        try
        {
            if (File.Exists(filePath + ".old")) File.Delete(filePath + "old");
            if (File.Exists(filePath)) File.Move(filePath, filePath + ".old");
        }
        catch (Exception exception)
        {
            CopyAssetError(asset, exception.Message);
            yield break;
        }
        
        var persistTask = File.WriteAllBytesAsync(filePath, www.downloadHandler.data);
        var hasError = false;
        while (!persistTask.IsCompleted)
        {
            if (persistTask.Exception != null)
            {
                hasError = true;
                CopyAssetError(asset, persistTask.Exception.Message);
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        
        www.downloadHandler.Dispose();
        www.Dispose();

        if (!hasError)
        {
            CopyAssetEnd(asset);
        }
    }

    private void LoadReleasesError(string error)
    {
        Specimen.Instance.Log.LogError($"[AutoUpdateMod] {Config.RepositoryOwner}/{Config.RepositoryName}: {error}");
    }

    private void NewUpdateAvailable(GithubRelease release)
    {
        LatestRelease = release;
    }

    private void DownloadAssetStart(GithubAsset asset)
    {
    }
    
    private void DownloadAssetProgress(GithubAsset asset, float progress)
    {
    }
    
    private void DownloadAssetError(GithubAsset asset, string error)
    {
        Specimen.Instance.Log.LogError($"[AutoUpdateMod]DownloadAssetError {Config.RepositoryOwner}/{Config.RepositoryName}: {error}");
    }
    
    private void DownloadAssetEnd(GithubAsset asset)
    {
    }
    
    private void CopyAssetStart(GithubAsset asset)
    {
    }
    
    private void CopyAssetError(GithubAsset asset, string error)
    {
        Specimen.Instance.Log.LogError($"[AutoUpdateMod]CopyAssetError {Config.RepositoryOwner}/{Config.RepositoryName}: {error}");
    }
    
    private void CopyAssetEnd(GithubAsset asset)
    {
    }
    
    private bool IsUpdateAsset(GithubAsset asset)
    {
        return Config.FilesToUpdate.Any(x => Regex.IsMatch(asset.Name, x));
    }
    
    private static int CompareReleases(GithubRelease a, GithubRelease b)
    {
        return a.Version > b.Version ? -1 : b.Version > a.Version ? 1 : 0;
    }
    
    private static UnityWebRequest MakeWebRequest(UnityWebRequest.UnityWebRequestMethod method, string url)
    {
        var www = new UnityWebRequest();
        www.SetMethod(method);
        www.SetUrl(url);
        www.downloadHandler = new DownloadHandlerBuffer();
        return www;
    }
}