using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.Utils;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;
using UniverseLib.UI;

namespace AmongUsSpecimen.Updater;

public class AutoUpdatedMod
{
    internal static readonly List<AutoUpdatedMod> AutoUpdatedMods = new();
    public bool HasUpdateAvailable => LatestRelease != null;
    
    public readonly UpdateModConfig Config;
    private UpdaterWindow _window;
    public List<GithubRelease> Releases;
    public GithubRelease LatestRelease { get; private set; }

    public AutoUpdatedMod(UpdateModConfig config)
    {
        Config = config;
        AutoUpdatedMods.Add(this);
    }

    public void ToggleWindow()
    {
        if (_window == null) return;
        _window.SetActive(!_window.Enabled);
    }

    public IEnumerator CoStart()
    {
        Releases = null;
        yield return CoLoadReleases();
        if (Releases == null) yield break;
        _window = UiManager.RegisterWindow<UpdaterWindow>(this);
        //_window = new UpdaterWindow(UiManager.UIBase, this);
        _window.SetActive(true);
    }

    private IEnumerator CoLoadReleases()
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
        Specimen.Instance.Log.LogMessage($"CoDownloadRelease: {release.Name}");
        foreach (var asset in release.Assets)
        {
            if (!IsUpdateAsset(asset)) continue;
            Specimen.Instance.Log.LogMessage($"CoDownloadRelease asset: {asset.Name}");
            yield return CoDownloadAsset(asset);
        }
    }
    
    private IEnumerator CoDownloadAsset(GithubAsset asset)
    {
        var dirPath = Path.Combine(Paths.PluginPath, Config.Directory);
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
        
        var filePath = Path.Combine(dirPath, asset.Name);
        try
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            else
            {
                Specimen.Instance.Log.LogMessage($"Delete old asset: {filePath}.old");
                if (File.Exists(filePath + ".old")) File.Delete(filePath + "old");
                Specimen.Instance.Log.LogMessage($"Move current asset: {filePath}");
                if (File.Exists(filePath)) File.Move(filePath, filePath + ".old");
            }
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
        _window.SetProgressBarActive(true);
        _window.SetProgressInfosText($"Downloading {asset.Name}");
    }
    
    private void DownloadAssetProgress(GithubAsset asset, float progress)
    {
        _window.SetDownloadProgression(progress);
    }
    
    private void DownloadAssetError(GithubAsset asset, string error)
    {
        Specimen.Instance.Log.LogError($"[AutoUpdateMod]DownloadAssetError {Config.RepositoryOwner}/{Config.RepositoryName}: {error}");
        _window.SetProgressInfosText(ColorHelpers.Colorize(Color.red, $"Error in {asset.Name} download:\n {error}"));
    }
    
    private void DownloadAssetEnd(GithubAsset asset)
    {
        _window.SetProgressInfosText($"{asset.Name} downloaded!");
        _window.SetDownloadProgression(1f);
    }
    
    private void CopyAssetStart(GithubAsset asset)
    {
        _window.SetProgressInfosText($"Updating {asset.Name}");
    }
    
    private void CopyAssetError(GithubAsset asset, string error)
    {
        Specimen.Instance.Log.LogError($"[AutoUpdateMod]CopyAssetError {Config.RepositoryOwner}/{Config.RepositoryName}: {error}");
        _window.SetProgressInfosText(ColorHelpers.Colorize(Color.red, $"Error in {asset.Name} update:\n {error}"));
    }
    
    private void CopyAssetEnd(GithubAsset asset)
    {
        _window.SetProgressInfosText(ColorHelpers.Colorize(UIPalette.Info, $"{asset.Name} successfully updated! Update will be applied after a game restart"));
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