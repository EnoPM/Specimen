using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.Components;

public sealed class BasicNotification : Notification
{
    
    private readonly BasicNotificationConfig Config;
    private Text _title { get; set; }
    private Text _description { get; set; }
    
    public BasicNotification(BasicNotificationConfig config, DateTime? expiresAt): base($"Notification_{config.Type.ToString()}", expiresAt)
    {
        Config = config;
    }

    public override void ConstructContent()
    {
        base.ConstructContent();
        var color = Config.Color;
        var textColor = color * 2f;
        ProgressBar.SetBarColor(color);
        ProgressBar.SetContainerColor(color * 0.6f);
        
        var container = UiFactory.CreateVerticalGroup(ContentRoot, "Container", false, false, true, true, bgColor: UIPalette.Transparent,
            childAlignment: TextAnchor.UpperLeft, padding: new Vector4(5f, 5f, 5f, 5f), spacing: 5);
        UiFactory.SetLayoutElement(container, Width, Height - 10, 0, 0);
        container.GetComponent<Image>().enabled = false;
        
        _title = UiFactory.CreateLabel(container, "Title", Config.Title,
            TextAnchor.MiddleLeft, Color.white);
        _title.fontSize = 16;
        _title.fontStyle = FontStyle.Bold;
        UiFactory.SetLayoutElement(_title.gameObject, Width - 10, 16, 0, 0);
        
        _description = UiFactory.CreateLabel(container, "Description", Config.Description,
            TextAnchor.MiddleLeft, textColor);
        _description.fontSize = 14;
        UiFactory.SetLayoutElement(_description.gameObject, Width - 10, 16, 0, 0);
    }
    
}

public enum NotificationTypes
{
    Success,
    Info,
    Danger,
    Warning,
}

public class BasicNotificationConfig
{
    public NotificationTypes Type;
    public string Title;
    public string Description;

    public Color Color => Type switch
    {
        NotificationTypes.Success => UIPalette.Success,
        NotificationTypes.Info => UIPalette.Info,
        NotificationTypes.Danger => UIPalette.Danger,
        NotificationTypes.Warning => UIPalette.Warning,
        _ => UIPalette.Transparent
    };
}