using System;
using System.Collections.Generic;

namespace AmongUsSpecimen.ModOptions;

public sealed class SelectionsController
{
    private readonly Func<List<string>> _valuesGetter;
    private readonly List<string> _values;

    public SelectionsController(Func<List<string>> getter)
    {
        _valuesGetter = getter;
        _values = null;
    }
    
    public SelectionsController(List<string> values)
    {
        _values = values;
        _valuesGetter = null;
    }

    public List<string> Values => _values ?? _valuesGetter();
    public int MinSelection => Values.Count >= 0 ? 0 : -1;
    public int MaxSelection => Values.Count - 1;

    public bool HasIndex(int index) => index >= 0 && index < Values.Count;
    public string GetValue(int index) => HasIndex(index) ? Values[index] : null;
}