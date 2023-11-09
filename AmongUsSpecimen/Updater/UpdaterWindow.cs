using System.Collections.Generic;
using System.Linq;
using AmongUsSpecimen.UI;
using AmongUsSpecimen.UI.Components;
using AmongUsSpecimen.UI.Extensions;
using AmongUsSpecimen.Utils;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.Updater;

public class UpdaterWindow : UiWindow
{
    private readonly AutoUpdatedMod UpdatedMod;
    
    public UpdaterWindow(AutoUpdatedMod update)
    {
        UpdatedMod = update;
        _selectedRelease = UpdatedMod.LatestRelease;
        
        BuildWindow();
    }

    public override string Name => "Specimen Updater";
    public override bool HasOverlay => true;
    protected override bool DisplayByDefault => true;
    public override bool AlwaysOnTop => true;
    protected override string Title => $"<b><color=#5925b3>Specimen</color> Updater:</b>";
    public override int MinWidth => 600;
    public override int MinHeight => 600;
    protected override Color BackgroundColor => UIPalette.Dark;
    protected override Positions Position => Positions.MiddleCenter;
    protected override void ConstructWindowContent()
    {
        
    }

    private Text _categoryTitle = null!;

    private GameObject _titleContainer;
    private GameObject _dropdownObject;
    private Text _releaseName;
    private Text _releaseDescription;
    private ButtonRef _downloadButton;
    private UI.Components.ProgressBar _progressBar;
    private Text _progressInfos;
    
    private readonly Dictionary<string, GithubRelease> _githubReleases = new();
    private GithubRelease _selectedRelease { get; set; }
    
    private void BuildWindow()
    {
        _titleContainer = UiFactory.CreateHorizontalGroup(ContentRoot, "TitleContainer", false, false, true, true,
            0, new Vector4(5f, 5f, 5f, 0f), UIPalette.Transparent);
        UiFactory.SetLayoutElement(_titleContainer, minHeight: 40, flexibleHeight: 0, minWidth: MinWidth,
            flexibleWidth: 0);
        
        _categoryTitle = UiFactory.CreateLabel(_titleContainer, "Title", "Select a version to install:", TextAnchor.MiddleLeft,
            UIPalette.Secondary, true, 18);
        _categoryTitle.fontStyle = FontStyle.Bold;
        UiFactory.SetLayoutElement(_categoryTitle.gameObject, minWidth: 260, flexibleWidth: 0, minHeight: 40,
            flexibleHeight: 0);
        
        var releaseNameContainer = UiFactory.CreateHorizontalGroup(ContentRoot, "ReleaseNameContainer", false, false, true, true,
            0, new Vector4(5f, 5f, 5f, 0f), UIPalette.Transparent, TextAnchor.MiddleCenter);
        UiFactory.SetLayoutElement(releaseNameContainer, minHeight: 35, flexibleHeight: 0, minWidth: MinWidth,
            flexibleWidth: 0);
        _releaseName = UiFactory.CreateLabel(releaseNameContainer, "ReleaseTitle", string.Empty, TextAnchor.MiddleCenter, fontSize: 30);
        UiFactory.SetLayoutElement(_releaseName.gameObject, MinWidth, flexibleWidth: 0, minHeight: 35, flexibleHeight: 0);
        
        var buttonsContainer = UiFactory.CreateVerticalGroup(ContentRoot, "ButtonsContainer", false, false, true, true,
            0, new Vector4(5f, 5f, 5f, 0f), UIPalette.Transparent, TextAnchor.MiddleCenter);
        UiFactory.SetLayoutElement(buttonsContainer, minHeight: 60, flexibleHeight: 0, minWidth: MinWidth,
            flexibleWidth: 0);
        _downloadButton = UiFactory.CreateButton(buttonsContainer, "DownloadButton", "Download & Install", UIPalette.Success);
        UiFactory.SetLayoutElement(_downloadButton.GameObject, 300, flexibleWidth: 0, minHeight: 30, flexibleHeight: 0);
        _downloadButton.Component.SetColorsAuto(UIPalette.Success);
        _downloadButton.ButtonText.fontSize = 18;
        _downloadButton.ButtonText.fontStyle = FontStyle.Bold;
        _downloadButton.OnClick = OnDownloadButtonClick;

        _progressBar = new UI.Components.ProgressBar(buttonsContainer, MinWidth - 40, 30);
        _progressBar.SetActive(false);

        _progressInfos = UiFactory.CreateLabel(buttonsContainer, "ProgressInfos", string.Empty, TextAnchor.MiddleCenter, UIPalette.Secondary);
        UiFactory.SetLayoutElement(_progressInfos.gameObject, MinWidth, flexibleWidth: 0, minHeight: 40, flexibleHeight: 0);
        _progressInfos.gameObject.SetActive(false);
        
        var scrollbarContainer = UiFactory.CreateVerticalGroup(ContentRoot, "ScrollbarContainer", false, false, true,
            true, 0, new Vector4(10f, 0f, 0f, 0f), UIPalette.Transparent);
        UiFactory.SetLayoutElement(scrollbarContainer, minHeight: 400, flexibleHeight: 0, minWidth: MinWidth,
            flexibleWidth: 0);
        
        var scrollerObject = UiFactory.CreateScrollView(scrollbarContainer, "ReleaseScrollView", out var content, out _);
        UiFactory.SetLayoutElement(scrollerObject, MinWidth, 25, 0, 9999);
        UiFactory.SetLayoutElement(content, MinWidth, 25, 0, 9999);
        
        _releaseDescription = UiFactory.CreateLabel(content, "ReleaseDescription", string.Empty, TextAnchor.UpperLeft, fontSize: 18);
        UiFactory.SetLayoutElement(_releaseDescription.gameObject, MinWidth, flexibleWidth: 0, minHeight: 40, flexibleHeight: 0);
        
        Header.SetText($"{Title} {UpdatedMod.Config.RepositoryOwner}/{UpdatedMod.Config.RepositoryName}");
        
        RefreshDropdown(UpdatedMod.LatestRelease);
    }
    
    public void SetProgressInfosText(string text)
    {
        _progressInfos.text = text;
    }

    public void SetProgressBarActive(bool value)
    {
        _progressInfos.gameObject.SetActive(value);
        _progressBar.SetActive(value);
        _downloadButton.GameObject.SetActive(!value);
    }

    private void OnDownloadButtonClick()
    {
        if (_selectedRelease == null || !UpdatedMod.Config.CheckCompatibility(UpdatedMod.Config, _selectedRelease)) return;
        UiManager.Behaviour.StartCoroutine(UpdatedMod.CoDownloadRelease(_selectedRelease));
    }

    public void SetDownloadProgression(float progression)
    {
        _progressBar?.SetProgression(progression);
    }

    private void RefreshSelectedRelease()
    {
        if (_selectedRelease == null) return;
        var isCompatible = UpdatedMod.Config.CheckCompatibility(UpdatedMod.Config, _selectedRelease);
        var isUpgrade = _selectedRelease.IsNewer(UpdatedMod.Config.VersionToCompare);
        var isReinstall = !isUpgrade && _selectedRelease.Version == UpdatedMod.Config.VersionToCompare;
        var updateType = isUpgrade ? "Upgrade" : isReinstall ? "Reinstall" : "Downgrade";
        var updateTypeColor = isUpgrade ? Color.green : isReinstall ? UIPalette.Info : UIPalette.Warning;
        var updateTypeArrow = isUpgrade ? "\u25b2" : isReinstall ? "=" : "\u25bc";
        _releaseName.text = $"{ColorHelpers.Colorize(updateTypeColor, updateType)}: {ColorHelpers.Colorize(Color.yellow, $"v{UpdatedMod.Config.VersionToCompare.ToString()}")} {ColorHelpers.Colorize(updateTypeColor, updateTypeArrow)} {_selectedRelease.Tag}";
        if (!isCompatible)
        {
            _releaseDescription.text = _selectedRelease.Description;
            _downloadButton.ButtonText.text = "Unsupported game version";
            _downloadButton.Component.SetColorsAuto(UIPalette.Danger);
        }
        else if (isUpgrade)
        {
            var releases = _githubReleases.Values.Where(r => r.IsNewer(UpdatedMod.Config.VersionToCompare) && !r.IsNewer(_selectedRelease.Version));
            _releaseDescription.text = string.Join("\n", releases.Select(r => r.Description));
            _downloadButton.Component.SetColorsAuto(UIPalette.Success);
            _downloadButton.ButtonText.text = $"Install {_selectedRelease.Name}";
        }
        else
        {
            _releaseDescription.text = _selectedRelease.Description;
            if (isReinstall)
            {
                _downloadButton.Component.SetColorsAuto(UIPalette.Info);
                _downloadButton.ButtonText.text = $"Reinstall {_selectedRelease.Tag}";
            }
            else
            {
                _downloadButton.Component.SetColorsAuto(UIPalette.Warning);
                _downloadButton.ButtonText.text = $"Downgrade {_selectedRelease.Tag}";
            }
        }
    }

    private void RefreshDropdown(GithubRelease latestRelease = null)
    {
        if (_dropdownObject) Object.Destroy(_dropdownObject);
        var defaultItemText = string.Empty;
        _githubReleases.Clear();
        if (UpdatedMod.Releases != null)
        {
            foreach (var release in UpdatedMod.Releases)
            {
                var isLatest = latestRelease?.Id == release.Id;
                var isCompatible = UpdatedMod.Config.CheckCompatibility(UpdatedMod.Config, release);
                var isCurrent = release.Version == UpdatedMod.Config.VersionToCompare;
                var isNewer = release.IsNewer(UpdatedMod.Config.VersionToCompare);
                var color = isCurrent ? UIPalette.Info : !isCompatible ? Color.red : isNewer ? Color.green : UIPalette.Warning;
                var text = release.Tag;
                if (isCurrent)
                {
                    text += " (current)";
                }
                else if (!isCompatible)
                {
                    text += " (incompatible)";
                }
                else if (isLatest)
                {
                    text += " (latest)";
                }

                var key = ColorHelpers.Colorize(color, text);

                _githubReleases[key] = release;
                if (isLatest)
                {
                    defaultItemText = key;
                }
            }
        }
        _dropdownObject = UiFactory.CreateDropdown(
            _titleContainer, 
            "VersionSelector", 
            out _, 
            defaultItemText, 
            16,
            OnVersionDropdownChanged,
            _githubReleases.Keys.ToArray());
        UiFactory.SetLayoutElement(_dropdownObject, minWidth: 275, flexibleWidth: 0, minHeight: 40, flexibleHeight: 0);
        if (latestRelease != null)
        {
            _selectedRelease = latestRelease;
        } else if (_githubReleases.Count > 0)
        {
            _selectedRelease = _githubReleases.First().Value;
        }
        RefreshSelectedRelease();
    }

    private void OnVersionDropdownChanged(int value)
    {
        var key = _githubReleases.Keys.ToArray()[value];
        _selectedRelease = _githubReleases[key];
        RefreshSelectedRelease();
    }
}