using UnityEngine;

namespace AmongUsSpecimen.UI;

internal class UiBehaviour : MonoBehaviour
{
    private int _cachedHeight;
    private int _cachedWidth;

    private void Start()
    {
        _cachedHeight = Screen.height;
        _cachedWidth = Screen.width;
    }

    private void Update()
    {
        var height = Screen.height;
        var width = Screen.width;
        if (height == _cachedHeight && width == _cachedWidth) return;
        Specimen.Instance.Log.LogMessage($"Screen size updated!");
        _cachedHeight = height;
        _cachedWidth = width;
        UiManager.EnsureWindowValidPositions();
    }
}