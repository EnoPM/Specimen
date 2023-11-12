using System;
using System.Collections.Generic;
using System.Linq;
using AmongUsSpecimen.UI.Components;
using UnityEngine;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI;

public static class NotificationManager
{
    internal static NotificationsWindow Window { get; private set; }

    internal static readonly List<Notification> Notifications = new();

    internal static void Start()
    {
        Window = UiManager.RegisterWindow<NotificationsWindow>();
    }

    internal static void UiUpdate()
    {
        var canExpire = new List<Notification>(Notifications.Where(x => x.ExpiresAt != null));
        foreach (var notification in canExpire)
        {
            if (notification.ExpiresAt <= DateTime.UtcNow)
            {
                notification.Remove();
                continue;
            }
            
            notification.RefreshProgressBar();
        }
    }

    public static T AddNotification<T>(T notification) where T : Notification
    {
        Notifications.Add(notification);
        notification.ConstructContent();
        return notification;
    }

    internal static BasicNotification DemoNotification(NotificationTypes type = NotificationTypes.Info, int duration = 30)
    {
        return AddNotification(new BasicNotification(new BasicNotificationConfig
        {
            Type = type,
            Title = "It works!",
            Description = "Congratulations! You have successfully installed your Among Us mod."
        }, DateTime.UtcNow.Add(TimeSpan.FromSeconds(duration))));
    }
}