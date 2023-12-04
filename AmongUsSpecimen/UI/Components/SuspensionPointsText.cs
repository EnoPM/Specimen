using System;
using AmongUsSpecimen.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace AmongUsSpecimen.UI.Components;

[RegisterMonoBehaviour]
public class SuspensionPointsText : MonoBehaviour
{
    public Text Text { get; set; }
    public float Interval { get; set; } = 1f;
    public string Point { get; set; } = ".";

    private float _timer;
    private uint _state;
    private string _text = string.Empty;

    public SuspensionPointsText(IntPtr ptr) : base(ptr)
    {
    }

    public void SetText(string text)
    {
        _text = text;
        RefreshText();
    }

    public void Start()
    {
        Text = gameObject.GetComponent<Text>();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer < 0f)
        {
            _timer = Interval;
            Progress();
        }
    }

    private string GetSuffix()
    {
        var suffix = string.Empty;
        if (_state > 0)
        {
            for (var i = 1; i <= _state; i++)
            {
                suffix += Point;
            }
        }

        return suffix;
    }

    private void RefreshText()
    {
        if (!Text) return;
        Text.text = $"{_text}{GetSuffix()}";
    }

    private void Progress()
    {
        _state = _state == 3 ? 0 : _state + 1;
        RefreshText();
    }
}