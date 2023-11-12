using AmongUsSpecimen.UI.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.Components;

public sealed class DownloadListNotification : Notification
{
    private SuspensionPointsText _description { get; set; }
    private int _fileCount { get; set; }
    private int _downloadFileCount { get; set; }
    private ButtonRef _button;

    public bool PauseDownload { get; private set; }
    
    public string Title { get; set; }
    
    public DownloadListNotification(int fileCount, int downloadFileCount, string title) : base(nameof(DownloadListNotification), null)
    {
        _fileCount = fileCount;
        _downloadFileCount = downloadFileCount;
        Title = title;
    }

    public override void ConstructContent()
    {
        base.ConstructContent();
        var color = UIPalette.Info;
        var textColor = color * 1.5f;
        ProgressBar.SetBarColor(color);
        ProgressBar.SetContainerColor(color * 0.6f);
        
        var container = UiFactory.CreateVerticalGroup(ContentRoot, "Container",
            false, false,
            true, true,
            bgColor: UIPalette.Transparent,
            spacing: 5,
            childAlignment: TextAnchor.UpperLeft,
            padding: new Vector4(5f, 5f, 5f, 5f));
        UiFactory.SetLayoutElement(container, Width, Height - 10, 0, 0);
        container.GetComponent<Image>().enabled = false;
        
        var title = UiFactory.CreateLabel(container, "Title", Title,
            TextAnchor.MiddleLeft, UIPalette.Secondary);
        title.fontSize = 16;
        title.fontStyle = FontStyle.Bold;
        UiFactory.SetLayoutElement(title.gameObject, Width - 10, 16, 0, 0);
        
        var label = UiFactory.CreateLabel(container, "Description", string.Empty,
            TextAnchor.MiddleLeft, textColor);
        label.fontSize = 14;
        UiFactory.SetLayoutElement(label.gameObject, Width - 10, 16, 0, 0);
        
        _description = label.gameObject.AddComponent<SuspensionPointsText>();
        _description.SetText(Title);
        _description.Interval = 0.4f;

        _button = UiFactory.CreateButton(container, "PauseButton", string.Empty);
        _button.OnClick = OnButtonClick;
        _button.ButtonText.color = Color.white;
        _button.ButtonText.fontStyle = FontStyle.Bold;
        UiFactory.SetLayoutElement(_button.GameObject, 240, 30, 0, 0);
        UpdateButton();
    }

    private void OnButtonClick()
    {
        PauseDownload = !PauseDownload;
        UpdateButton();
        UpdateProgression();
    }
    
    private void UpdateButton()
    {
        _button.Component.SetColorsAuto(PauseDownload ? UIPalette.Success : UIPalette.Warning);
    }
    
    public void UpdateProgression()
    {
        ProgressBar.SetProgression(_downloadFileCount, _fileCount);
        _button.ButtonText.text = $"{(PauseDownload ? "Resume download" : "Pause download")} [{_downloadFileCount}/{_fileCount}]";
        var color = PauseDownload ? UIPalette.Warning * 1.5f : UIPalette.Success * 1.5f;
        ProgressBar.SetContainerColor(color * 0.5f);
        ProgressBar.SetBarColor(color);
        if (_description.Text != null)
        {
            _description.Text.color = color;
        }
        if (PauseDownload)
        {
            _description.SetText($"{_fileCount - _downloadFileCount} files remaining (paused).");
            _description.Point = string.Empty;
        }
        else
        {
            _description.SetText($"{_fileCount - _downloadFileCount} files remaining");
            _description.Point = ".";
        }
    }
    
    public void IncrementDownloadFile()
    {
        _downloadFileCount++;
        UpdateProgression();
    }
}