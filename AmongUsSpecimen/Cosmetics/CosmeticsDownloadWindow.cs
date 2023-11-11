using AmongUsSpecimen.UI;
using AmongUsSpecimen.UI.Components;
using AmongUsSpecimen.UI.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.Cosmetics;

public class CosmeticsDownloadWindow : UiWindow
{
    public override string Name => nameof(CosmeticsDownloadWindow);
    public override int MinWidth => 250;
    public override int MinHeight => 85;
    public override bool DisableGameClickWhenOpened => false;
    protected override bool DisplayTitleBar => false;
    protected override Color BackgroundColor => UIPalette.Dark;
    //protected override Color BackgroundColor => Color.red;
    public override bool AlwaysOnTop => false;
    protected override bool DisplayByDefault => false;
    protected override Positions Position => Positions.MiddleRight;

    private UI.Components.ProgressBar _progressBar { get; set; }
    private SuspensionPointsText _title { get; set; }
    private int _fileCount { get; set; }
    private int _downloadFileCount { get; set; }
    private ButtonRef _button;
    
    protected override void ConstructWindowContent()
    {
        _progressBar = new UI.Components.ProgressBar(ContentRoot, MinWidth, 10, Vector4.zero);
        _progressBar.PercentageText.enabled = false;
        _progressBar.SetBarColor(Color.green);
        _progressBar.SetContainerColor(UIPalette.Dark);
        _progressBar.SetActive(true);
        
        var container = UiFactory.CreateVerticalGroup(ContentRoot, "Container", false, false, true, true, bgColor: UIPalette.Transparent,
            childAlignment: TextAnchor.UpperLeft, padding: new Vector4(5f, 5f, 5f, 5f));
        UiFactory.SetLayoutElement(container, MinWidth, MinHeight - 10, 0, 0);
        container.GetComponent<Image>().enabled = false;
        
        var label = UiFactory.CreateLabel(container, "Title", string.Empty,
            TextAnchor.MiddleLeft, UIPalette.Secondary);
        label.fontSize = 16;
        UiFactory.SetLayoutElement(label.gameObject, MinWidth, 30, 0, 0);
        
        _title = label.gameObject.AddComponent<SuspensionPointsText>();
        _title.SetText("Downloading asset 0/0");
        _title.Interval = 0.4f;

        _button = UiFactory.CreateButton(container, "PauseButton", string.Empty);
        _button.OnClick = OnButtonClick;
        _button.ButtonText.color = Color.white;
        _button.ButtonText.fontStyle = FontStyle.Bold;
        UiFactory.SetLayoutElement(_button.GameObject, 150, 30, 0, 0);
        UpdateButton();
    }

    private void OnButtonClick()
    {
        CustomCosmeticsManager.PauseDownloader = !CustomCosmeticsManager.PauseDownloader;
        UpdateButton();
    }

    private void UpdateButton()
    {
        var paused = CustomCosmeticsManager.PauseDownloader;
        _button.Component.SetColorsAuto(paused ? UIPalette.Success : UIPalette.Warning);
        _button.ButtonText.text = paused ? "Resume download" : "Pause download";
    }

    private void UpdateProgression()
    {
        _progressBar.SetProgression(_downloadFileCount, _fileCount);
        _title.SetText($"Downloading asset {_downloadFileCount}/{_fileCount}");
    }

    public void InitProgression(int fileCount, int downloadFileCount)
    {
        _fileCount = fileCount;
        _downloadFileCount = downloadFileCount;
        UpdateProgression();
    }

    public void NextAsset()
    {
        _downloadFileCount++;
        UpdateProgression();
    }
}