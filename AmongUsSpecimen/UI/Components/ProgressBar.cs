﻿using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace AmongUsSpecimen.UI.Components;

public class ProgressBar
{
    public readonly GameObject Container;
    public readonly GameObject Bar;
    public readonly Text PercentageText;

    private readonly LayoutElement _barLayout;
    private readonly float _maxBarWidth;

    public ProgressBar(GameObject root, int width, int height, Vector4? containerPadding = null)
    {
        _maxBarWidth = width - 2;
        Container = UiFactory.CreateHorizontalGroup(root, "ProgressBarContainer",
            false, true, true, true, 0,
            containerPadding ?? Vector4.one, UIPalette.Secondary, TextAnchor.MiddleLeft);

        UiFactory.SetLayoutElement(Container, minWidth: width, minHeight: height, flexibleHeight: 0);

        Bar = UiFactory.CreateHorizontalGroup(Container, "ProgressBar", false, true, false, true,
            bgColor: UIPalette.Info, childAlignment: TextAnchor.MiddleCenter);
        _barLayout = UiFactory.SetLayoutElement(Bar, minWidth: 1, minHeight: height - 2);
        PercentageText =
            UiFactory.CreateLabel(Bar, "PercentageText", "0%", TextAnchor.MiddleCenter, UIPalette.Secondary, true, 14);
        PercentageText.horizontalOverflow = HorizontalWrapMode.Overflow;
        PercentageText.gameObject.SetActive(false);
    }

    public void SetActive(bool value)
    {
        Container.SetActive(value);
    }

    public void SetProgression(float percentage)
    {
        if (percentage > 1f) percentage = 1f;
        else if (percentage < 0f) percentage = 0f;

        _barLayout.minWidth = percentage * _maxBarWidth;

        PercentageText.text = $"{Mathf.Round(percentage * 100f)}%";
        PercentageText.gameObject.SetActive(_barLayout.minWidth >= 30);
    }

    public void SetProgression(float min, float max)
    {
        if (min > max) return;
        SetProgression(min / max);
    }

    public void SetContainerColor(Color color)
    {
        Container.GetComponent<Image>().color = color;
    }

    public void SetBarColor(Color color)
    {
        Bar.GetComponent<Image>().color = color;
    }
}