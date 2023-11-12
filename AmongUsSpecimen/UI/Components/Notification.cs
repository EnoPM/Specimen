using System;
using UnityEngine;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.Components;

public abstract class Notification
{
    public const int Width = 250;
    public const int Height = 85;
    
    private readonly DateTime CreatedAt = DateTime.UtcNow;
    public readonly DateTime? ExpiresAt;
    protected readonly GameObject ContentRoot;

    protected virtual bool AutoRefreshProgressBar => true;
    
    protected ProgressBar ProgressBar { get; set; }

    protected Notification(string name, DateTime? expiresAt)
    {
        ContentRoot = NotificationManager.Window.CreateNotificationObject(name);
        ExpiresAt = expiresAt;
    }
    
    public void Remove()
    {
        UnityEngine.Object.Destroy(ContentRoot);
        NotificationManager.Notifications.Remove(this);
    }

    public virtual void ConstructContent()
    {
        ProgressBar = new ProgressBar(ContentRoot, Width, 5, Vector4.zero);
        ProgressBar.PercentageText.enabled = false;
        ProgressBar.SetBarColor(UIPalette.Secondary);
        ProgressBar.SetContainerColor(UIPalette.Primary);
        ProgressBar.SetProgression(0f);
        ProgressBar.SetActive(true);
    }

    internal void RefreshProgressBar()
    {
        if (!AutoRefreshProgressBar || !ExpiresAt.HasValue) return;
        var createdAt = new DateTimeOffset(CreatedAt).ToUnixTimeMilliseconds();
        var expiresAt = new DateTimeOffset(ExpiresAt.Value).ToUnixTimeMilliseconds();
        var total = expiresAt - createdAt;
        var now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        var pastTime = now - createdAt;
        ProgressBar.SetProgression((float)pastTime / total);
    }
}
